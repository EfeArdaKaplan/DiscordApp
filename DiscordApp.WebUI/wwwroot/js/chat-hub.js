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
    hubConnection.on("ChannelCreated", (channel) => {
        const server = serversData.find(s => s.id === channel.serverId);
        if (!server) return;

        if (!server.channels) server.channels = [];
        if (server.channels.some(c => c.id === channel.id)) return;

        server.channels.push(channel);

        if (currentView === 'server' && currentServerId === channel.serverId) {
            renderChannels(server);
        }
    });

    hubConnection.on("VoiceChannelUsersUpdated", (channelId, users) => {
        renderVoiceUsers(channelId, users);
    });

    hubConnection.on("ReceiveChannelMessage", (message) => {
        // message: { messageId, senderId, senderUserName, content, sentAt, channelId }

        const isCurrentChannelOpen =
            currentView === 'server' &&
            currentChannelId !== null &&
            String(message.channelId) === String(currentChannelId);

        if (isCurrentChannelOpen) {
            appendSingleMessage(message);
        }
        // İstersen buraya kanal listesinde "okunmamış" rozeti eklenebilir (markDMAsUnread'e benzer şekilde)
    });

    // reconnect sonrası, o an açık olan kanalın grubuna tekrar katıl
    hubConnection.onreconnected(() => {
        if (currentView === 'server' && currentChannelId !== null) {
            joinChannelGroup(currentChannelId);
        }
    });

    hubConnection.start()
        .then(() => {
            console.log("SignalR bağlantısı kuruldu.");
            // bağlantı kurulduğunda zaten açık bir kanal varsa (nadir ama olabilir) gruba katıl
            if (currentView === 'server' && currentChannelId !== null) {
                joinChannelGroup(currentChannelId);
            }
        })
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

function joinChannelGroup(channelId) {
    if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) return;
    hubConnection.invoke('JoinChannel', channelId).catch(err => console.error('JoinChannel hatası:', err));
}

function leaveChannelGroup(channelId) {
    if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) return;
    hubConnection.invoke('LeaveChannel', channelId).catch(err => console.error('LeaveChannel hatası:', err));
}

document.addEventListener('DOMContentLoaded', () => {
    startHubConnection();
});

