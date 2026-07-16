let hubConnection = null;

function startHubConnection() {
    if (!window.JWT_TOKEN) {
        console.warn("JWT_TOKEN yok, SignalR bağlantısı kurulamıyor.");
        return;
    }

    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(window.HUB_URL, {
            accessTokenFactory: () => window.JWT_TOKEN
        })
        .withAutomaticReconnect()
        .build();

    hubConnection.on("ReceiveMessage", (message) => {
        // message: { messageId, senderId, senderUserName, content, sentAt }

        const isCurrentConversationOpen =
            currentView === 'dm' &&
            currentDMFriendId !== null &&
            String(message.senderId) === String(currentDMFriendId);

        if (isCurrentConversationOpen) {
            appendSingleMessage(message);
        } else {
            // Şu an açık olan sohbetten farklı biri yazdıysa, DM listesinde küçük bir işaret göster
            markDMAsUnread(message.senderId);
        }
    });

    hubConnection.start()
        .then(() => console.log("SignalR bağlantısı kuruldu."))
        .catch(err => console.error("SignalR bağlantı hatası:", err));
}

function markDMAsUnread(senderId) {
    const friend = friendsData.find(f => String(f.id) === String(senderId));
    if (!friend) return;

    // dmList içindeki ilgili butonu bulup basit bir nokta/rozet ekle (CSS'in varsa kullan, yoksa metinle işaretle)
    const buttons = document.querySelectorAll('#dmList .dm-item');
    buttons.forEach(btn => {
        if (btn.textContent.includes(friend.userName)) {
            btn.classList.add('has-unread'); // site.css'e .has-unread stili ekleyebilirsin
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    startHubConnection();
});