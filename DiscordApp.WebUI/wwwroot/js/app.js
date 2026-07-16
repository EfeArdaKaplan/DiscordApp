// Home/Index ekranı: sunucu/DM listesi, kanal seçimi, mesajlaşma, ses ekranı ve ayarlar.
// window.CURRENT_USER ve window.SERVERS_DATA değerleri Index.cshtml içinde sunucudan basılır.
let userName = window.CURRENT_USER || "Kullanıcı";
let serversData = window.SERVERS_DATA || [];
let friendsData = window.FRIENDS_DATA || [];
let currentServerId = null;
let currentThread = "";
let currentView = "dm"; // 'dm' ya da 'server'
let currentDMFriendId = null;
// Kanal mesajlaşması henüz gerçek API'ye bağlı değil, o yüzden şimdilik kanal başına boş bir mock depo tutuyoruz.
let channelMessageStore = {};

function showPage(name) {
    document.querySelectorAll('.page').forEach(p => p.classList.add('hidden'));
    document.getElementById('page-' + name).classList.remove('hidden');
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
        return;
    }

    channels.forEach((ch, index) => {
        const btn = document.createElement('button');
        btn.className = 'chan-btn' + (index === 0 ? ' active' : '');
        btn.innerHTML = '<span class="icon">#</span>' + escapeHtml(ch.name);
        btn.onclick = () => selectChannel(btn, ch.name);
        list.appendChild(btn);
    });

    // İlk kanalı otomatik seç
    currentThread = channels[0].name;
    document.getElementById('chanIcon').textContent = '#';
    document.getElementById('chanTitle').textContent = channels[0].name;
    if (!channelMessageStore[currentThread]) channelMessageStore[currentThread] = [];
    renderMessages();
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

function selectChannel(el, name) {
    document.querySelectorAll('#sidebar-server .chan-btn').forEach(b => b.classList.remove('active'));
    el.classList.add('active');
    currentThread = name;
    document.getElementById('chanIcon').textContent = '#';
    document.getElementById('chanTitle').textContent = name;
    if (!channelMessageStore[currentThread]) channelMessageStore[currentThread] = [];
    renderMessages();
}

function renderDMList() {
    const list = document.getElementById('dmList');
    list.innerHTML = '';

    if (friendsData.length === 0) {
        list.innerHTML = '<p class="section-label">Henüz arkadaşın yok, ＋ ile ekleyebilirsin.</p>';
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
        <p>${escapeHtml(m.content)}</p>
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
        <p>${escapeHtml(m.content)}</p>
      </div>`;
        box.appendChild(d);
    });

    box.scrollTop = box.scrollHeight;
}

function goVoice(roomName) {
    document.getElementById('voiceRoomName').textContent = roomName;
    showPage('voice');
}

function renderMessages() {
    const box = document.getElementById('messages');
    if (!box) return;
    box.innerHTML = '';
    (channelMessageStore[currentThread] || []).forEach(m => {
        if (m.system) {
            const d = document.createElement('div');
            d.className = 'msg-system';
            d.innerHTML = '<span>▸</span>' + m.text;
            box.appendChild(d);
        } else {
            const d = document.createElement('div');
            d.className = 'msg';
            d.innerHTML = `<div class="msg-avatar">${initials(m.author)}</div>
        <div class="msg-body">
          <div class="msg-meta"><span class="msg-author">${m.author}</span><span class="msg-time">${m.time}</span></div>
          <p>${m.text}</p>
        </div>`;
            box.appendChild(d);
        }
    });
    box.scrollTop = box.scrollHeight;
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
        input.value = ''; // önce inputu temizle, kullanıcı beklemesin

        const res = await callApi('/Home/SendMessage', 'POST', { receiverId: currentDMFriendId, content: val });

        if (res.success) {
            // Sunucuya gidip gelmeden, kendi mesajını anında ekrana bas
            appendSingleMessage({
                senderId: window.CURRENT_USER_ID,
                senderUserName: window.CURRENT_USER,
                content: val,
                sentAt: new Date().toISOString()
            });
        } else {
            input.value = val; // başarısızsa yazdığını geri koy
            alert(res.message || 'Mesaj gönderilemedi.');
        }
    } else {
        alert('Kanal mesajlaşması henüz aktif değil, bir sonraki adımda ekleyeceğiz.');
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
    renderMessages();
    renderDMList(); // Varsayılan görünüm DM olduğu için arkadaş listesini hemen doldur

    // '+' butonunu bul ve Model açma olayını bağla
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

async function submitNewChannel() {
    const input = document.getElementById("newChannelNameInput");
    const channelName = input ? input.value.trim() : "";

    if (!channelName) {
        alert("Lütfen bir kanal adı girin!");
        return;
    }
    if (!currentServerId) {
        alert("Önce bir sunucu seç!");
        return;
    }

    try {
        const response = await fetch('/Home/CreateChannel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ serverId: currentServerId, name: channelName })
        });

        const resData = await response.json();

        if (resData.success) {
            closeChannelModal();

            // Sayfayı yenilemek yerine mevcut sunucu verisine yeni kanalı dinamik ekleyip render alıyoruz
            const currentServer = serversData.find(s => s.id === currentServerId);
            if (currentServer) {
                if (!currentServer.channels) currentServer.channels = [];
                currentServer.channels.push({
                    id: Date.now(), // Geçici dinamik ID
                    name: channelName,
                    serverId: currentServerId
                });
                renderChannels(currentServer);
            }
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
setInterval(checkPendingRequests, 15000); // her 15 saniyede bir kontrol et