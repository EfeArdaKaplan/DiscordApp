// Home/Index ekranı: sunucu/DM listesi, kanal seçimi, mesajlaşma, ses ekranı ve ayarlar.
// window.CURRENT_USER değeri Index.cshtml içinde sunucudan (session'daki kullanıcı adından) basılır.
let userName = window.CURRENT_USER || "Kullanıcı";
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
    ],
    "genel": [
        { system: true, text: "Kanal oluşturuldu: #genel" },
        { author: "Arda", time: "09:10", text: "Günaydın ekip!" }
    ],
    "proje-notlari": [
        { author: "Sofia", time: "10:00", text: "Frontend taslağını ekledim." }
    ],
    "hata-ayiklama": [
        { author: "Deniz", time: "13:40", text: "Composer'da enter tuşu çalışmıyor, bakıyorum." }
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
        document.getElementById('sidebar-server').classList.remove('hidden');
        document.getElementById('sidebar-dms').classList.add('hidden');
        document.getElementById('members-panel').classList.remove('hidden');
        selectChannel(document.querySelector('#sidebar-server .chan-btn'), 'genel');
    }
}

function selectChannel(el, name) {
    document.querySelectorAll('#sidebar-server .chan-btn').forEach(b => b.classList.remove('active'));
    el.classList.add('active');
    currentThread = name;
    document.getElementById('chanIcon').textContent = '#';
    document.getElementById('chanTitle').textContent = name;
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

// SUNUCU EKLEME POPUP (MODAL) İŞLEMLERİ

document.addEventListener('DOMContentLoaded', () => {
    renderMessages();

    // '+' butonunu bul ve Model açma olayını bağla
    const addBtn = document.querySelector(".rail-add");
    if (addBtn) {
        addBtn.addEventListener("click", openServerModal);
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
            location.reload(); // Sunucuyu sol tarafda göstermek için yenile
        } else {
            alert(resData.message || "Sunucu oluşturulurken bir hata oluştu.");
        }
    } catch (err) {
        console.error("Hata:", err);
        alert("Sunucu eklenirken bir bağlantı hatası oluştu!");
    }
}