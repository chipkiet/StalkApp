window.WebRTC = {
    dotNetRef: null,
    peerConnection: null,
    localStream: null,
    remoteStream: null,
    isVideo: false,

    config: {
        iceServers: [
            { urls: 'stun:stun.l.google.com:19302' },
            { urls: 'stun:stun1.l.google.com:19302' }
        ]
    },

    init: async function (dotNetReference, isVideoCall) {
        this.dotNetRef = dotNetReference;
        this.isVideo = isVideoCall;
        
        try {
            this.localStream = await navigator.mediaDevices.getUserMedia({
                video: isVideoCall,
                audio: true
            });

            const localVideo = document.getElementById('localVideo');
            if (localVideo) {
                localVideo.srcObject = this.localStream;
                localVideo.muted = true; // Always mute local video
            }

            return true;
        } catch (error) {
            console.warn('Lỗi truy cập Camera/Mic (có thể do đang test 2 tab trên 1 máy). Thử fallback sang chỉ dùng Audio...', error);
            if (isVideoCall) {
                try {
                    this.localStream = await navigator.mediaDevices.getUserMedia({
                        video: false, // Fallback: tắt video
                        audio: true   // Vẫn thử lấy audio (thường mic cho phép dùng chung)
                    });
                    
                    const localVideo = document.getElementById('localVideo');
                    if (localVideo) {
                        localVideo.srcObject = this.localStream;
                        localVideo.muted = true;
                    }
                    return true;
                } catch (fallbackError) {
                    console.error('Fallback Audio cũng thất bại.', fallbackError);
                    return false;
                }
            }
            return false;
        }
    },

    setupPeerConnection: function () {
        this.peerConnection = new RTCPeerConnection(this.config);

        // Add local tracks to peer connection
        this.localStream.getTracks().forEach(track => {
            this.peerConnection.addTrack(track, this.localStream);
        });

        // Listen for remote tracks
        this.peerConnection.ontrack = (event) => {
            const remoteVideo = document.getElementById('remoteVideo');
            if (remoteVideo) {
                if (remoteVideo.srcObject !== event.streams[0]) {
                    remoteVideo.srcObject = event.streams[0];
                    console.log('Remote stream received');
                }
            }
        };

        // Listen for ICE candidates and send them to the other peer via Blazor
        this.peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                const payload = JSON.stringify({ type: 'candidate', candidate: event.candidate });
                this.dotNetRef.invokeMethodAsync('SendSignal', payload);
            }
        };
    },

    startCall: async function () {
        this.setupPeerConnection();
        try {
            const offer = await this.peerConnection.createOffer();
            await this.peerConnection.setLocalDescription(offer);
            const payload = JSON.stringify({ type: 'offer', offer: offer });
            this.dotNetRef.invokeMethodAsync('SendSignal', payload);
        } catch (error) {
            console.error('Error creating offer.', error);
        }
    },

    handleSignal: async function (payloadStr) {
        if (!this.peerConnection) {
            this.setupPeerConnection();
        }

        try {
            const payload = JSON.parse(payloadStr);

            if (payload.type === 'offer') {
                await this.peerConnection.setRemoteDescription(new RTCSessionDescription(payload.offer));
                const answer = await this.peerConnection.createAnswer();
                await this.peerConnection.setLocalDescription(answer);
                const answerPayload = JSON.stringify({ type: 'answer', answer: answer });
                this.dotNetRef.invokeMethodAsync('SendSignal', answerPayload);
            } else if (payload.type === 'answer') {
                await this.peerConnection.setRemoteDescription(new RTCSessionDescription(payload.answer));
            } else if (payload.type === 'candidate') {
                await this.peerConnection.addIceCandidate(new RTCIceCandidate(payload.candidate));
            }
        } catch (error) {
            console.error('Error handling signal.', error);
        }
    },

    toggleAudio: function (enabled) {
        if (this.localStream) {
            this.localStream.getAudioTracks().forEach(track => {
                track.enabled = enabled;
            });
        }
    },

    toggleVideo: function (enabled) {
        if (this.localStream) {
            this.localStream.getVideoTracks().forEach(track => {
                track.enabled = enabled;
            });
        }
    },

    endCall: function () {
        if (this.peerConnection) {
            this.peerConnection.close();
            this.peerConnection = null;
        }
        if (this.localStream) {
            this.localStream.getTracks().forEach(track => track.stop());
            this.localStream = null;
        }
        const localVideo = document.getElementById('localVideo');
        if (localVideo) localVideo.srcObject = null;
        
        const remoteVideo = document.getElementById('remoteVideo');
        if (remoteVideo) remoteVideo.srcObject = null;
        
        if (this.dotNetRef) {
            this.dotNetRef = null;
        }
    }
};
