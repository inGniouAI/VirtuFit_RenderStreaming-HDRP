import { Signaling, WebSocketSignaling } from "./signaling.js";
import { Observer, Sender } from "./sender.js";
import { InputRemoting } from "./inputremoting.js";
import Peer from "./peer.js";
import * as Logger from "./logger.js";
import { sendInputTextEvent } from "./register-events.js";


function uuid4() {
  var temp_url = URL.createObjectURL(new Blob());
  var uuid = temp_url.toString();
  URL.revokeObjectURL(temp_url);
  return uuid.split(/[:/]/g).pop().toLowerCase(); // remove prefixes
}

function isTouchDevice() {
  return (('ontouchstart' in window) ||
    (navigator.maxTouchPoints > 0) ||
    (navigator.msMaxTouchPoints > 0));
}


export class VirtualFitReceiver {
  constructor(videoElement) {
    const _this = this;
    this.pc = null;
    this.channel = null;
    this.connectionId = null;

    // this.sender = new Sender(videoElement);
    // this.sender.addMouse();
    // this.sender.addKeyboard();
    // if (isTouchDevice()) {
    //   this.sender.addTouchscreen();
    // }
    // this.sender.addGamepad();
    // this.inputRemoting = new InputRemoting(this.sender);

    this.localStream = new MediaStream();
    this.video = videoElement;
    this.video.playsInline = true;
    this.video.addEventListener('loadedmetadata', function () {
      _this.video.play();
      _this.resizeVideo();
    }, true);
    this.video.srcObject = this.localStream;
    //this.maxVideoTrackLength = 2;

    this.ondisconnect = function () { };
    this.onconnect = function () { };

  }

  async setupConnection(useWebSocket) {
    const _this = this;
    // close current RTCPeerConnection
    if (this.pc) {
      Logger.log('Close current PeerConnection');
      this.pc.close();
      this.pc = null;
    }

    if (useWebSocket) {
      this.signaling = new WebSocketSignaling();
    } else {
      this.signaling = new Signaling();
    }

    this.connectionId = uuid4();

    // Create peerConnection with proxy server and set up handlers
    this.pc = new Peer(this.connectionId, true);
    this.pc.addEventListener('disconnect', () => {
      _this.ondisconnect();
    });
    this.pc.addEventListener('connect', () => {
      _this.onconnect();
      console.Log("data chennle connect");
      console.Log("data chennle connect peer pc sdp "+ this.pc.localDescription.sdp);

    });
    this.pc.addEventListener('trackevent', (e) => {
      const data = e.detail;
      if (data.track.kind == 'video') {
        _this.localStream.addTrack(data.track);
      }
      if (data.track.kind == 'audio') {
        _this.localStream.addTrack(data.track);
      }
    });
    this.pc.addEventListener('sendoffer', (e) => {
      const cEvent = e.detail;
      _this.signaling.sendOffer(cEvent.connectionId, cEvent.sdp);
      console.log("cEvent.sdp "+cEvent.sdp);
    });
    this.pc.addEventListener('sendCustomEvent', (e) => {
      const offer = e.detail;
      _this.signaling.sendCustomEvent(offer.connectionId, offer.sdp);
      console.log("sendCustomEvent ");
    });
    this.pc.addEventListener('sendanswer', (e) => {
      const answer = e.detail;
      _this.signaling.sendAnswer(answer.connectionId, answer.sdp);
    });
    this.pc.addEventListener('sendcandidate', (e) => {
      const candidate = e.detail;
      _this.signaling.sendCandidate(candidate.connectionId, candidate.candidate, candidate.sdpMid, candidate.sdpMLineIndex);
    });

    this.signaling.addEventListener('disconnect', async (e) => {
      const data = e.detail;
      if (_this.pc != null && _this.pc.connectionId == data.connectionId) {
        _this.ondisconnect();
      }
    });
    this.signaling.addEventListener('offer', async (e) => {
      const offer = e.detail;
      const desc = new RTCSessionDescription({ sdp: offer.sdp, type: "offer" });
      if (_this.pc != null) {
        await _this.pc.onGotDescription(offer.connectionId, desc);
      }
    });
    this.signaling.addEventListener('answer', async (e) => {
      const answer = e.detail;
      const desc = new RTCSessionDescription({ sdp: answer.sdp, type: "answer" });
      if (_this.pc != null) {
        await _this.pc.onGotDescription(answer.connectionId, desc);
      }
    });
    this.signaling.addEventListener('candidate', async (e) => {
      const candidate = e.detail;
      const iceCandidate = new RTCIceCandidate({ candidate: candidate.candidate, sdpMid: candidate.sdpMid, sdpMLineIndex: candidate.sdpMLineIndex });
      if (_this.pc != null) {
        await _this.pc.onGotCandidate(candidate.connectionId, iceCandidate);
      }
    });

    // setup signaling
    await this.signaling.start();

    // kick send offer process
    // this.inputSenderChannel = this.pc.createDataChannel(this.connectionId, "data");
    // this.inputSenderChannel.onopen = this._onOpenInputSenderChannel.bind(this);
    // this.inputRemoting.subscribe(new Observer(this.inputSenderChannel));

    // Create data channel with proxy server and set up handlers
    this.channel = this.pc.createDataChannel(this.connectionId, 'data');
    this.channel.onopen = function () {
      Logger.log('Datachannel connected.');
      _this.onconnect();
    };
    this.channel.onerror = function (e) {
      Logger.log("The error " + e.error.message + " occurred\n while handling data with proxy server.");
    };
    this.channel.onclose = function () {
      console.log("this.channel.onclose with send custom event");
      Logger.log('Datachannel disconnected.');
    };
    this.channel.onmessage = async (msg) => {
      // receive message from unity and operate message
      let data;
      // receive message data type is blob only on Firefox
      if (navigator.userAgent.indexOf('Firefox') != -1) {
        data = await msg.data.arrayBuffer();
      } else {
        data = msg.data;
      }
      const bytes = new Uint8Array(data);
      _this.videoTrackIndex = bytes[1];
      switch (bytes[0]) {
        case UnityEventType.SWITCH_VIDEO:
          _this.switchVideo(_this.videoTrackIndex);
          break;
      }
    };
  }

  resizeVideo() {
    const clientRect = this.video.getBoundingClientRect();
    const videoRatio = this.videoWidth / this.videoHeight;
    const clientRatio = clientRect.width / clientRect.height;

    this._videoScale = videoRatio > clientRatio ? clientRect.width / this.videoWidth : clientRect.height / this.videoHeight;
    const videoOffsetX = videoRatio > clientRatio ? 0 : (clientRect.width - this.videoWidth * this._videoScale) * 0.5;
    const videoOffsetY = videoRatio > clientRatio ? (clientRect.height - this.videoHeight * this._videoScale) * 0.5 : 0;
    this._videoOriginX = clientRect.left + videoOffsetX;
    this._videoOriginY = clientRect.top + videoOffsetY;
  }

  get videoWidth() {
    return this.video.videoWidth;
  }

  get videoHeight() {
    return this.video.videoHeight;
  }

  get videoOriginX() {
    return this._videoOriginX;
  }

  get videoOriginY() {
    return this._videoOriginY;
  }

  get videoScale() {
    return this._videoScale;
  }

  async _onOpenInputSenderChannel() {
    await new Promise(resolve => setTimeout(resolve, 100));
    this.inputRemoting.startSending();
  }
  sendMsg(msg) {
    if (this.channel == null) {
      return;
    }
    switch (this.channel.readyState) {
      case 'connecting':
        Logger.log('Connection not ready');
        break;
      case 'open':
        this.channel.send(msg);
        break;
      case 'closing':
        Logger.log('Attempt to sendMsg message while closing');
        break;
      case 'closed':
        Logger.log('Attempt to sendMsg message while connection closed.');
        break;
    }
  }
  async stop() {
    if (this.pc != null) {
      console.log("Stop Window");
      this.pc.dispatchEvent(new CustomEvent('sendCustomEvent', { detail: { connectionId: this.connectionId } }));
    //  this.pc.dispatchEvent(new CustomEvent('Customevent', { detail: { connectionId: this.connectionId } }));

    }
    if (this.signaling) {
      await this.signaling.stop();
      this.signaling = null;
    }

    if (this.pc) {
    
      this.pc.close();
      this.pc = null;
    }
  }
  
}

