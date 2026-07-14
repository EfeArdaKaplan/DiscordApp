// Home/Index ekranı: sunucu/DM listesi, kanal seçimi, mesajlaşma, ses ekranı ve ayarlar.
// window.CURRENT_USER ve window.SERVERS_DATA değerleri Index.cshtml içinde sunucudan basılır.
let userName = window.CURRENT_USER || "Kullanıcı";
let serversData = window.SERVERS_DATA || []; // [{id, name, channels:[{id,name,serverId}]}]
let currentServerId = null;
let currentThread = "Arda";
let messageStore = {
    "Arda": [
        { author: "Arda", time: "14:02", text: "Bugünkü hedef: sayfa geçişlerini bitirmek." },
        { author: "Sofia", time: "14:03", text: "Tamamdır, önce giriş ekranını hallediyorum." }
    ],
    "Deniz": [
        { author: "Deniz", time: "11:20", text: "Kanal listesi tasarımını beğendim." }
    ],
    "Staj Grubu": [
        { system: true, text: "Yerel sunucu çalışıyor · localhost:4000" },
        { author: "Arda", time: "14:05", text: "Ses entegrasyonunu sonraya bırakıyoruz, önce arayüz." },
        { author: "Mira (bot)", time: "14:05", text: "Not alındı." }
    ]
};

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
        selectDM(document.querySelector('#sidebar-dms .dm-item'), 'Arda', false);
    } else {
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
    }""

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
    if (!messageStore[currentThread]) messageStore[currentThread] = [];
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
    if (!messageStore[currentThread]) messageStore[currentThread] = [];
    renderMessages();
}

function selectDM(el, name, isGroup) {
    document.querySelectorAll('#sidebar-dms .dm-item').forEach(b => b.classList.remove('active'));
    el.classList.add('active');
    currentThread = name;
    document.getElementById('chanIcon').textContent = isGroup ? '👥' : '👤';
    document.getElementById('chanTitle').textContent = name;
    renderMessages();
}

function goVoice(roomName) {
    document.getElementById('voiceRoomName').textContent = roomName;
    showPage('voice');
}

function renderMessages() {
    const box = document.getElementById('messages');
    if (!box) return;
    box.innerHTML = '';
    (messageStore[currentThread] || []).forEach(m => {
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

function sendMessage() {
    const input = document.getElementById('draftInput');
    const val = input.value.trim();
    if (!val) return;
    const now = new Date();
    const time = now.getHours().toString().padStart(2, '0') + ':' + now.getMinutes().toString().padStart(2, '0');
    if (!messageStore[currentThread]) messageStore[currentThread] = [];
    messageStore[currentThread].push({ author: userName, time, text: val });
    input.value = '';
    renderMessages();
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