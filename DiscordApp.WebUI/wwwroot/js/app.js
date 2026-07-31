// Home/Index ekranı: sunucu/DM listesi, kanal seçimi, mesajlaşma, ses ekranı ve ayarlar.
// window.CURRENT_USER ve window.SERVERS_DATA değerleri Index.cshtml içinde sunucudan basılır.
let userName = window.CURRENT_USER || "Kullanıcı";
let serversData = window.SERVERS_DATA || [];
let friendsData = window.FRIENDS_DATA || [];
let currentServerId = null;
let currentThread = "";
let currentView = "dm"; // 'dm' ya da 'server'
let currentDMFriendId = null;
let currentChannelId = null;
let mediaRecorder = null;
let audioChunks = [];
let recordingStartTime = null;
let recordingStream = null;

function buildMessageBodyHtml(m) {
    if (m.type === 'Voice') {
        // Eski kayıtlar göreli yol (/uploads/...) olabilir, yenileri MinIO'dan tam URL (http://...) geliyor
        let src = m.audioUrl;
        if (src && !src.startsWith('http')) {
            const apiBase = window.HUB_URL.replace('/hubs/chat', '');
            src = apiBase + src;
        }
        return `<audio controls src="${src}" style="height:32px; max-width:220px; margin-top:2px;"></audio>`;
    }
    return `<p>${linkify(escapeHtml(m.content))}</p>`;
}
// voice işlemleri

async function toggleVoiceRecording() {
    if (mediaRecorder && mediaRecorder.state === 'recording') {
        stopVoiceRecording();
        return;
    }

    const canSendDM = currentView === 'dm' && currentDMFriendId;
    const canSendChannel = currentView === 'server' && currentChannelId;

    if (!canSendDM && !canSendChannel) {
        alert('Sesli mesaj göndermek için bir arkadaş ya da kanal seçmelisin.');
        return;
    }
    try {
        recordingStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    } catch (err) {
        alert('Mikrofona erişilemedi. Tarayıcı izinlerini kontrol et.');
        return;
    }

    audioChunks = [];
    mediaRecorder = new MediaRecorder(recordingStream, { mimeType: 'audio/webm;codecs=opus' });
    mediaRecorder.ondataavailable = (e) => { if (e.data.size > 0) audioChunks.push(e.data); };
    mediaRecorder.onstop = onRecordingStopped;

    recordingStartTime = Date.now();
    mediaRecorder.start();

    const btn = document.getElementById('voiceBtn');
    btn.textContent = '⏹';
    btn.style.color = '#E5484D';
}

function stopVoiceRecording() {
    if (mediaRecorder && mediaRecorder.state === 'recording') mediaRecorder.stop();
    if (recordingStream) recordingStream.getTracks().forEach(t => t.stop());

    const btn = document.getElementById('voiceBtn');
    btn.textContent = '🎤';
    btn.style.color = 'var(--text-mut)';
}

async function onRecordingStopped() {
    const durationSeconds = Math.round((Date.now() - recordingStartTime) / 1000);
    if (durationSeconds < 1) return;

    const blob = new Blob(audioChunks, { type: 'audio/webm' });
    const formData = new FormData();
    formData.append('Audio', blob, 'voice.webm');
    formData.append('DurationSeconds', durationSeconds);

    const isChannel = currentView === 'server' && currentChannelId;

    if (isChannel) {
        formData.append('ChannelId', currentChannelId);
    } else {
        formData.append('ReceiverId', currentDMFriendId);
    }

    const apiBase = window.HUB_URL.replace('/hubs/chat', '');

    try {
        const response = await fetch(`${apiBase}/api/Message/SendVoice`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${window.JWT_TOKEN}` },
            body: formData
        });
        const data = await response.json();
        if (data.success) {
            if (!isChannel) {
                appendSingleMessage(data.message);
            }
        } else {
            alert(data.message || 'Sesli mesaj gönderilemedi.');
        }
    } catch (err) {
        console.error(err);
        alert('Sesli mesaj gönderilirken bağlantı hatası oluştu.');
    }
}

function showPage(name) {
    document.querySelectorAll('.page').forEach(p => p.classList.add('hidden'));
    document.getElementById('page-' + name).classList.remove('hidden');
}

// davet etme
async function inviteFriendToCurrentServer(receiverId, btn) {
    btn.disabled = true;
    const res = await fetch('/Friend/InviteToServer', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ serverId: currentServerId, receiverId })
    });
    const data = await res.json();
    btn.textContent = data.success ? 'Gönderildi ✓' : (data.message || 'Hata');
    if (!data.success) btn.disabled = false;
}

function switchRailView(view, el) {
    document.querySelectorAll('.rail-btn').forEach(b => b.classList.remove('active'));
    el.classList.add('active');
    if (view === 'dms') {
        document.getElementById('sidebar-dms').classList.remove('hidden');
        document.getElementById('sidebar-server').classList.add('hidden');
        document.getElementById('members-panel').classList.add('hidden');
        currentView = 'dm';
        renderDMList();
    }
    else {

        const serverId = parseInt(el.getAttribute('data-server-id'), 10);
        currentServerId = serverId;
        currentView = 'server';

        document.getElementById('sidebar-server').classList.remove('hidden');
        document.getElementById('sidebar-dms').classList.add('hidden');
        document.getElementById('members-panel').classList.remove('hidden');

        loadServer(serverId);
    }
}

// Seçilen sunucunun kanallarını (zaten elimizde olan veriden) ve üyelerini (API'den) yükler.
function loadServer(serverId) {
    const server = serversData.find(s => s.id === serverId);
    document.getElementById('currentServerName').textContent = server ? server.name : 'Sunucu';

    renderChannels(server);
    loadMembers(serverId);
}

function renderChannels(server) {
    const list = document.getElementById('channelList');
    list.innerHTML = '';

    const channels = (server && server.channels) ? server.channels : [];

    if (channels.length === 0) {
        const empty = document.createElement('p');
        empty.className = 'section-label';
        empty.textContent = 'Henüz kanal yok';
        list.appendChild(empty);
        document.getElementById('messages').innerHTML = '<p class="msg-system"><span>▸</span>Bu sunucuda henüz kanal yok.</p>';
        return;
    }

    const textChannels = channels.filter(c => c.type !== 'Voice');
    const voiceChannels = channels.filter(c => c.type === 'Voice');

    let firstTextBtn = null;

    if (textChannels.length > 0) {
        const header = document.createElement('p');
        header.className = 'section-label';
        header.textContent = 'METİN KANALLARI';
        list.appendChild(header);

        textChannels.forEach((ch) => {
            const btn = document.createElement('button');
            btn.className = 'chan-btn';
            btn.innerHTML = '<span class="icon">#</span>' + escapeHtml(ch.name);
            btn.onclick = () => selectChannel(btn, ch);
            list.appendChild(btn);
            if (!firstTextBtn) firstTextBtn = btn;
        });
    }

    if (voiceChannels.length > 0) {
        const header = document.createElement('p');
        header.className = 'section-label';
        header.textContent = 'SES KANALLARI';
        list.appendChild(header);

        voiceChannels.forEach((ch) => {
            const btn = document.createElement('button');
            btn.className = 'chan-btn';
            btn.innerHTML = '<span class="icon">🔊</span>' + escapeHtml(ch.name);
            btn.onclick = () => joinVoiceChannel(ch);
            list.appendChild(btn);
        });
    }

    if (firstTextBtn) {
        firstTextBtn.classList.add('active');
        selectChannel(firstTextBtn, textChannels[0]);
    }
}

async function loadMembers(serverId) {
    const list = document.getElementById('membersList');
    const label = document.getElementById('membersLabel');
    list.innerHTML = '<p class="section-label">Yükleniyor...</p>';

    try {
        const response = await fetch(`/Home/GetMembers?serverId=${serverId}`);
        if (!response.ok) throw new Error('Üyeler alınamadı');
        const members = await response.json();

        label.textContent = `Üyeler — ${members.length}`;
        list.innerHTML = '';

        members.forEach(m => {
            const row = document.createElement('div');
            row.className = 'member-row';
            row.innerHTML = `<span class="member-avatar">${initials(m.userName)}<span class="status-dot" style="background:var(--mint)"></span></span>
        <div><p class="member-name">${escapeHtml(m.userName)}</p><p class="member-role">${m.isOwner ? 'sunucu sahibi' : 'üye'}</p></div>`;
            list.appendChild(row);
        });
    } catch (err) {
        console.error(err);
        list.innerHTML = '<p class="section-label">Üyeler yüklenemedi</p>';
    }
}

function escapeHtml(str) {
    const d = document.createElement('div');
    d.textContent = str ?? '';
    return d.innerHTML;
}
function linkify(escapedText) {
    const urlRegex = /\b((?:https?:\/\/(?:localhost|(?:\d{1,3}\.){3}\d{1,3})(?::\d{1,5})?(?:\/[^\s<]*)?)|(?:(?:https?:\/\/)?(?:www\.)?[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}(?:\/[^\s<]*)?))\b/gi;

    return escapedText.replace(urlRegex, (match) => {
        let href = match;
        if (!href.startsWith('http://') && !href.startsWith('https://')) {
            href = 'https://' + href;
        }
        let trailing = '';
        const trailingMatch = href.match(/[.,!?;:)]+$/);
        if (trailingMatch) {
            trailing = trailingMatch[0];
            href = href.slice(0, -trailing.length);
            match = match.slice(0, -trailing.length);
        }
        return `<a href="${href}" target="_blank" rel="noopener noreferrer">${match}</a>${trailing}`;
    });
}

async function selectChannel(el, channel) {
    document.querySelectorAll('#sidebar-server .chan-btn').forEach(b => b.classList.remove('active'));
    el.classList.add('active');

    if (currentChannelId !== null && currentChannelId !== channel.id) {
        leaveChannelGroup(currentChannelId);
    }

    currentThread = channel.name;
    currentChannelId = channel.id;
    document.getElementById('chanIcon').textContent = '#';
    document.getElementById('chanTitle').textContent = channel.name;

    joinChannelGroup(channel.id);
    await loadChannelMessages(channel.id);
}

async function loadChannelMessages(channelId) {
    const box = document.getElementById('messages');
    box.innerHTML = '<p class="msg-system"><span>▸</span>Yükleniyor...</p>';

    try {
        const response = await fetch(`/Home/GetChannelMessages?channelId=${channelId}`);
        if (!response.ok) throw new Error('Mesajlar alınamadı');
        const messages = await response.json();
        renderRealMessages(messages);
    } catch (err) {
        console.error(err);
        box.innerHTML = '<p class="msg-system"><span>▸</span>Mesajlar yüklenemedi.</p>';
    }
}

function renderDMList() {
    const list = document.getElementById('dmList');
    list.innerHTML = '';

    if (friendsData.length === 0) {
        list.innerHTML = '<p class="section-label">Henüz arkadaşın yok, + ile ekleyebilirsin.</p>';
        return;
    }

    friendsData.forEach(friend => {
        const btn = document.createElement('button');
        btn.className = 'dm-item';
        btn.innerHTML = `<span class="dm-avatar">${initials(friend.userName)}<span class="status-dot" style="background:var(--mint)"></span></span>
      <span><span class="dm-name">${escapeHtml(friend.userName)}</span></span>`;
        btn.onclick = () => selectDM(btn, friend);
        list.appendChild(btn);
    });

    // Henüz kimse seçilmediği için orta ekranda boş/bilgilendirici bir mesaj göster
    document.getElementById('chanTitle').textContent = 'Bir arkadaş seç';
    document.getElementById('chanIcon').textContent = '👤';
    document.getElementById('messages').innerHTML = '<p class="msg-system"><span>▸</span>Sohbet açmak için soldan bir arkadaş seç.</p>';
    currentDMFriendId = null;
}
async function selectDM(el, friend) {
    document.querySelectorAll('#sidebar-dms .dm-item').forEach(b => b.classList.remove('active'));
    el.classList.add('active');

    currentThread = friend.userName;
    currentDMFriendId = friend.id;
    document.getElementById('chanIcon').textContent = '👤';
    document.getElementById('chanTitle').textContent = friend.userName;

    await loadConversation(friend.id);
}

async function loadConversation(friendId) {
    const box = document.getElementById('messages');
    box.innerHTML = '<p class="msg-system"><span>▸</span>Yükleniyor...</p>';

    try {
        const response = await fetch(`/Home/GetConversation?otherUserId=${friendId}`);
        const messages = await response.json();
        renderRealMessages(messages);
    } catch (err) {
        console.error(err);
        box.innerHTML = '<p class="msg-system"><span>▸</span>Mesajlar yüklenemedi.</p>';
    }
}

function appendSingleMessage(m) {
    const box = document.getElementById('messages');
    const time = new Date(m.sentAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    const d = document.createElement('div');
    d.className = 'msg';
    d.innerHTML = `<div class="msg-avatar">${initials(m.senderUserName)}</div>
      <div class="msg-body">
        <div class="msg-meta"><span class="msg-author">${escapeHtml(m.senderUserName)}</span><span class="msg-time">${time}</span></div>
        ${buildMessageBodyHtml(m)}
      </div>`;
    box.appendChild(d);
    box.scrollTop = box.scrollHeight;
}

function renderRealMessages(messages) {
    const box = document.getElementById('messages');
    box.innerHTML = '';

    if (messages.length === 0) {
        box.innerHTML = '<p class="msg-system"><span>▸</span>Henüz mesaj yok, ilk mesajı sen at.</p>';
        return;
    }

    messages.forEach(m => {
        const time = new Date(m.sentAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
        const d = document.createElement('div');
        d.className = 'msg';
        d.innerHTML = `<div class="msg-avatar">${initials(m.senderUserName)}</div>
      <div class="msg-body">
        <div class="msg-meta"><span class="msg-author">${escapeHtml(m.senderUserName)}</span><span class="msg-time">${time}</span></div>
        ${buildMessageBodyHtml(m)}
      </div>`;
        box.appendChild(d);
    });

    box.scrollTop = box.scrollHeight;
}

function goVoice(roomName) {
    document.getElementById('voiceRoomName').textContent = roomName;
    showPage('voice');
}

function initials(str) {
    if (!str) return "?";
    let parts = str.split(' ').filter(w => w.length > 0);
    let res = parts.map(w => w[0].toUpperCase()).join('');
    return res.substring(0, 2);
}

async function callApi(url, method = 'GET', body = null) {
    try {
        const options = { method };
        if (body) {
            options.headers = { 'Content-Type': 'application/json' };
            options.body = JSON.stringify(body);
        }
        const response = await fetch(url, options);
        return await response.json();
    } catch (err) {
        console.error(err);
        return { success: false, message: 'Bağlantı hatası oluştu!' };
    }
}

async function sendMessage() {
    const input = document.getElementById('draftInput');
    const val = input.value.trim();
    if (!val) return;

    if (currentView === 'dm' && currentDMFriendId) {
        input.value = '';
        const res = await callApi('/Home/SendMessage', 'POST', { receiverId: currentDMFriendId, content: val });

        if (res.success) {
            appendSingleMessage({
                senderId: window.CURRENT_USER_ID,
                senderUserName: window.CURRENT_USER,
                content: val,
                sentAt: new Date().toISOString()
            });
        } else {
            input.value = val;
            alert(res.message || 'Mesaj gönderilemedi.');
        }
    } else if (currentView === 'server' && currentChannelId) {
        input.value = '';
        const res = await callApi('/Home/SendChannelMessage', 'POST', { channelId: currentChannelId, content: val });

        if (!res.success) {
            input.value = val;
            alert(res.message || 'Mesaj gönderilemedi.');
        }
    }
}
function pickTheme(el) {
    document.querySelectorAll('.theme-pills .pill').forEach(p => p.classList.remove('active'));
    el.classList.add('active');
}

// SOL ALTTAKİ KULLANICI BİLGİLERİNİ GÜNCELLEME
function updateLocalUserProfile() {
    const userAvatarEl = document.getElementById("userAvatar");
    const userNameLabelEl = document.getElementById("userNameLabel");

    if (userNameLabelEl) {
        userNameLabelEl.textContent = userName;
    }
    if (userAvatarEl) {
        userAvatarEl.textContent = initials(userName);
    }
}

// SUNUCU EKLEME POPUP (MODAL) İŞLEMLERİ
document.addEventListener('DOMContentLoaded', () => {
    updateLocalUserProfile(); // Kullanıcı adını sol altta güncelle
    renderDMList(); // Varsayılan görünüm DM olduğu için arkadaş listesini hemen doldur

    // '+' butonunu bul ve Modal açma olayını bağla
    const addBtn = document.querySelector(".rail-add");
    if (addBtn) {
        addBtn.addEventListener("click", openServerModal);
    }

    // "kanal ekle" butonu
    const addChannelBtn = document.getElementById("addChannelBtn");
    if (addChannelBtn) {
        addChannelBtn.addEventListener("click", openChannelModal);
    }

    // Sayfa ilk açıldığında zaten seçili olan (ilk) sunucuyu yükle
    const firstServerBtn = document.querySelector('.rail-btn[data-server-id]');
    if (firstServerBtn && document.getElementById('sidebar-server') && !document.getElementById('sidebar-server').classList.contains('hidden')) {
        switchRailView('server', firstServerBtn);
    }
});
function openServerModal() {
    const modal = document.getElementById("createServerModal");
    if (modal) {
        modal.style.display = "flex";
        const input = document.getElementById("newServerNameInput");
        if (input) input.focus();
    }
}

function closeServerModal() {
    const modal = document.getElementById("createServerModal");
    if (modal) modal.style.display = "none";
    const input = document.getElementById("newServerNameInput");
    if (input) input.value = "";
}

async function submitNewServer() {
    const input = document.getElementById("newServerNameInput");
    const serverName = input ? input.value.trim() : "";

    if (!serverName) {
        alert("Lütfen bir sunucu adı girin!");
        return;
    }

    try {
        const response = await fetch('/Home/CreateServer', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: serverName })
        });

        const resData = await response.json();

        if (resData.success) {
            closeServerModal();
            location.reload(); // Sunucu eklendiğinde rail güncellenmesi için yenileme
        } else {
            alert(resData.message || "Sunucu oluşturulurken bir hata oluştu.");
        }
    } catch (err) {
        console.error("Hata:", err);
        alert("Sunucu eklenirken bir bağlantı hatası oluştu!");
    }
}

// KANAL EKLEME POPUP (MODAL) İŞLEMLERİ

function openChannelModal() {
    if (!currentServerId) {
        alert("Önce bir sunucu seç!");
        return;
    }
    const modal = document.getElementById("createChannelModal");
    if (modal) {
        modal.style.display = "flex";
        const input = document.getElementById("newChannelNameInput");
        if (input) input.focus();
    }
}

function closeChannelModal() {
    const modal = document.getElementById("createChannelModal");
    if (modal) modal.style.display = "none";
    const input = document.getElementById("newChannelNameInput");
    if (input) input.value = "";
}

function openInviteFriendModal() {
    if (!currentServerId) { alert('Önce bir sunucu seç.'); return; }

    const list = document.getElementById('inviteFriendList');
    list.innerHTML = '';

    if (friendsData.length === 0) {
        list.innerHTML = '<p style="color:#949ba4; font-size:13px;">Davet edebileceğin bir arkadaşın yok.</p>';
    } else {
        friendsData.forEach(friend => {
            const row = document.createElement('div');
            row.style.cssText = 'display:flex; align-items:center; justify-content:space-between; padding:8px 0; border-bottom:1px solid #3f4147;';
            row.innerHTML = `<span>${escapeHtml(friend.userName)}</span>
                <button style="background:#5865f2; border:none; color:#fff; padding:6px 12px; border-radius:4px; cursor:pointer; font-size:12px;">Davet Et</button>`;
            row.querySelector('button').onclick = (e) => inviteFriendToCurrentServer(friend.id, e.target);
            list.appendChild(row);
        });
    }

    document.getElementById('inviteFriendModal').style.display = 'flex';
}

function closeInviteFriendModal() {
    document.getElementById('inviteFriendModal').style.display = 'none';
}

async function inviteFriendToCurrentServer(receiverId, btn) {
    btn.disabled = true;
    btn.textContent = 'Gönderiliyor...';

    const res = await fetch('/Friend/InviteToServer', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ serverId: currentServerId, receiverId })
    });
    const data = await res.json();

    if (data.success) {
        btn.textContent = 'Gönderildi ✓';
    } else {
        btn.disabled = false;
        btn.textContent = 'Davet Et';
        alert(data.message || 'Davet gönderilemedi.');
    }
}

async function submitNewChannel() {
    const input = document.getElementById("newChannelNameInput");
    const channelName = input ? input.value.trim() : "";
    const typeInput = document.querySelector('input[name="newChannelType"]:checked');
    const channelType = typeInput ? typeInput.value : "Text";

    if (!channelName) { alert("Lütfen bir kanal adı girin!"); return; }
    if (!currentServerId) { alert("Önce bir sunucu seç!"); return; }

    try {
        const response = await fetch('/Home/CreateChannel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ serverId: currentServerId, name: channelName, type: channelType })
        });
        const resData = await response.json();

        if (resData.success) {
            closeChannelModal();
        } else {
            alert(resData.message || "Kanal oluşturulurken bir hata oluştu.");
        }
    } catch (err) {
        console.error("Hata:", err);
        alert("Kanal eklenirken bir bağlantı hatası oluştu!");
    }
}

const originalTitle = document.title;

async function checkPendingRequests() {
    try {
        const response = await fetch('/Friend/GetPendingCount');
        const data = await response.json();
        const badge = document.getElementById('pendingBadge');
        if (!badge) return;

        if (data.count > 0) {
            badge.textContent = data.count > 9 ? '9+' : data.count;
            badge.classList.remove('hidden');
            document.title = `(${data.count}) ${originalTitle}`;
        } else {
            badge.classList.add('hidden');
            document.title = originalTitle;
        }
    } catch (err) {
        console.error('Bildirim sayısı alınamadı:', err);
    }
}

checkPendingRequests();
setInterval(checkPendingRequests, 120000); // her 15 saniyede bir kontrol et

async function copyInviteLink() {
    if (!currentServerId) {
        alert('Önce bir sunucu seç.');
        return;
    }

    try {
        const res = await fetch(`/Home/GetInviteLink?serverId=${currentServerId}`);
        const data = await res.json();

        if (!data.success || !data.inviteCode) {
            alert(data.message || 'Davet linki alınamadı.');
            return;
        }

        const inviteUrl = `${window.location.origin}/invite/${data.inviteCode}`;

        await navigator.clipboard.writeText(inviteUrl);
        alert('Davet linki kopyalandı!');
    } catch (err) {
        console.error('Davet linki kopyalanamadı:', err);
        alert('Davet linki kopyalanırken bir hata oluştu.');
    }
}

let currentVoiceChannelId = null;

async function joinVoiceChannel(channel) {
    if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) {
        alert('Bağlantı hazır değil, birazdan tekrar dene.');
        return;
    }

    currentVoiceChannelId = channel.id;
    document.getElementById('voiceRoomName').textContent = channel.name;
    showPage('voice');

    try {
        await startLocalAudio(); // mikrofon izni burada istenir
    } catch (err) {
        alert('Mikrofona erişilemedi. Tarayıcı izinlerini kontrol et.');
        showPage('app');
        currentVoiceChannelId = null;
        return;
    }

    try {
        const existingPeers = await hubConnection.invoke('JoinVoiceChannel', channel.id);
        for (const peer of existingPeers) {
            await callPeer(peer.connectionId);
        }
    } catch (err) {
        console.error('JoinVoiceChannel hatası:', err);
        alert('Sesli kanala katılamadın.');
    }
}

function leaveVoiceChannel() {
    if (currentVoiceChannelId !== null && hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
        hubConnection.invoke('LeaveVoiceChannel', currentVoiceChannelId).catch(err => console.error(err));
    }
    closeAllPeerConnections();
    stopLocalAudio();
    isMicMuted = false;
    currentVoiceChannelId = null;
    showPage('app');
}

function renderVoiceUsers(channelId, users) {
    if (channelId !== currentVoiceChannelId) return;

    const grid = document.querySelector('#page-voice .voice-grid');
    if (!grid) return;
    grid.innerHTML = '';

    if (users.length === 0) {
        grid.innerHTML = '<p class="section-label">Bu kanalda henüz kimse yok.</p>';
    } else {
        users.forEach(u => {
            const tile = document.createElement('div');
            tile.className = 'voice-tile';
            tile.innerHTML = `<div class="voice-avatar">${initials(u.userName)}</div><p>${escapeHtml(u.userName)}</p>`;
            grid.appendChild(tile);
        });
    }

    const activeConnectionIds = users.map(u => u.connectionId);
    Object.keys(peerConnections).forEach(connId => {
        if (!activeConnectionIds.includes(connId)) {
            removePeerConnection(connId);
        }
    });
}