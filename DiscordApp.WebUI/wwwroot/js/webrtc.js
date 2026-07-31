const rtcConfig = {
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
};

let localStream = null;
const peerConnections = {}; // connectionId -> RTCPeerConnection

async function startLocalAudio() {
    if (localStream) return localStream;
    localStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    return localStream;
}

function stopLocalAudio() {
    if (localStream) {
        localStream.getTracks().forEach(t => t.stop());
        localStream = null;
    }
}

function createPeerConnection(remoteConnectionId) {
    const pc = new RTCPeerConnection(rtcConfig);

    if (localStream) {
        localStream.getTracks().forEach(track => pc.addTrack(track, localStream));
    }

    pc.onicecandidate = (event) => {
        if (event.candidate) {
            hubConnection.invoke('SendVoiceIceCandidate', remoteConnectionId, JSON.stringify(event.candidate))
                .catch(err => console.error('ICE candidate gönderilemedi:', err));
        }
    };

    pc.ontrack = (event) => {
        attachRemoteAudio(remoteConnectionId, event.streams[0]);
    };

    pc.onconnectionstatechange = () => {
        if (pc.connectionState === 'disconnected' || pc.connectionState === 'failed' || pc.connectionState === 'closed') {
            removePeerConnection(remoteConnectionId);
        }
    };

    peerConnections[remoteConnectionId] = pc;
    return pc;
}

function removePeerConnection(connectionId) {
    const pc = peerConnections[connectionId];
    if (pc) {
        pc.close();
        delete peerConnections[connectionId];
    }
    const audioEl = document.getElementById(`voice-audio-${connectionId}`);
    if (audioEl) audioEl.remove();
}

function attachRemoteAudio(connectionId, stream) {
    let audioEl = document.getElementById(`voice-audio-${connectionId}`);
    if (!audioEl) {
        audioEl = document.createElement('audio');
        audioEl.id = `voice-audio-${connectionId}`;
        audioEl.autoplay = true;
        document.body.appendChild(audioEl);
    }
    audioEl.srcObject = stream;
}

async function callPeer(remoteConnectionId) {
    const pc = createPeerConnection(remoteConnectionId);
    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);
    await hubConnection.invoke('SendVoiceOffer', remoteConnectionId, JSON.stringify(offer));
}

async function handleReceiveVoiceOffer(fromConnectionId, offerJson) {
    const offer = JSON.parse(offerJson);
    const pc = createPeerConnection(fromConnectionId);
    await pc.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);
    await hubConnection.invoke('SendVoiceAnswer', fromConnectionId, JSON.stringify(answer));
}

async function handleReceiveVoiceAnswer(fromConnectionId, answerJson) {
    const answer = JSON.parse(answerJson);
    const pc = peerConnections[fromConnectionId];
    if (pc) await pc.setRemoteDescription(new RTCSessionDescription(answer));
}

async function handleReceiveVoiceIceCandidate(fromConnectionId, candidateJson) {
    const candidate = JSON.parse(candidateJson);
    const pc = peerConnections[fromConnectionId];
    if (pc) {
        try { await pc.addIceCandidate(new RTCIceCandidate(candidate)); }
        catch (err) { console.error('ICE candidate eklenemedi:', err); }
    }
}

function closeAllPeerConnections() {
    Object.keys(peerConnections).forEach(removePeerConnection);
}

let isMicMuted = false;

function toggleMic(btn) {
    if (!localStream) return;

    isMicMuted = !isMicMuted;
    localStream.getAudioTracks().forEach(track => {
        track.enabled = !isMicMuted; // WebRTC bağlantısını bozmadan sesi kapatıp açar
    });

    btn.textContent = isMicMuted ? '🔇' : '🎤';
    btn.classList.toggle('muted', isMicMuted);
}