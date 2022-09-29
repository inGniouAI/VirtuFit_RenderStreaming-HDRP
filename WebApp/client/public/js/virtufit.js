import { Signaling, WebSocketSignaling } from "./signaling.js";
import { Observer, Sender } from "./sender.js";
import { InputRemoting } from "./inputremoting.js";
import Peer from "./peer.js";
import * as Logger from "./logger.js";
import { sendInputTextEvent } from "./register-events.js";
import { AvatarCode, SkuCode} from "./main.js"
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
var analyticSdp ;//= "http://www.example.com/landing.aspx?referrer=10.11.12.13an12.11.12.13";
var analyticIps = "";
var analyticDatetime = "" ;
var time1;
var time2;
var analyticTotalSpenttime;
SetAnalyticData();
function SetAnalyticData(){
  time1 = new Date();
  analyticDatetime = formatDate(new Date());
  console.log("analyticDatetime "+analyticDatetime);

}
function padTo2Digits(num) {
  return num.toString().padStart(2, '0');
}

function convertMsToTime(milliseconds) {
  let seconds = Math.floor(milliseconds / 1000);
  let minutes = Math.floor(seconds / 60);
 // let hours = Math.floor(minutes / 60);

  seconds = seconds % 60;
  minutes = minutes % 60;

  // 👇️ If you don't want to roll hours over, e.g. 24 to 00
  // 👇️ comment (or remove) the line below
  // commenting next line gets you `24:00:00` instead of `00:00:00`
  // or `36:15:31` instead of `12:15:31`, etc.
//  hours = hours % 24;

  return `${padTo2Digits(minutes)}:${padTo2Digits(
    seconds,
  )}`;
}

function formatDate(date) {
  return (
    [
      date.getFullYear(),
      padTo2Digits(date.getMonth() + 1),
      padTo2Digits(date.getDate()),
    ].join('/') +
    ' ' +
    [
      padTo2Digits(date.getHours()),
      padTo2Digits(date.getMinutes()),
      padTo2Digits(date.getSeconds()),
    ].join(':')
  );
}
function findIP(onNewIP,sdps) { //  onNewIp - your listener function for new IPs
  var noop = function() {};
  var localIPs = {};

  var ipRegex = /([0-9]{1,3}(\.[0-9]{1,3}){3}|[a-f0-9]{1,4}(:[a-f0-9]{1,4}){7})/g;
  var key;

  function ipIterate(ip) {
    if (!localIPs[ip]) onNewIP(ip);
    localIPs[ip] = true;
  }
  sdps.match(ipRegex).forEach(ipIterate);
}
function addIP(ip) {
  analyticIps +=";"+ip;
  console.log('got ip: ', analyticIps);

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
     // console.Log("data chennle connect peer pc sdp "+ this.pc.localDescription.sdp);
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
      analyticSdp = cEvent.sdp.toString();
      console.log("set analyticSdp "+analyticSdp);


    });
   // this.pc.addEventListener('sendCustomEvent', (e) => {
     // const offer = e.detail;
      //_this.signaling.sendCustomEvent(offer.connectionId, offer.sdp);
      //console.log("sendCustomEvent ");
    //});
    this.pc.addEventListener('sendAnalytic', (e) => {
      const analytic = e.detail;
      _this.signaling.sendAnalytic(analytic.connectionId,analytic.dateTimeId, analytic.sku, analytic.avatarCode,analytic.ipv4,analytic.totalTimeSpent);
      console.log("sendAnalytic ");
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
    time2 = new Date();
    console.log("analyticSdp "+analyticSdp);

    findIP(addIP,analyticSdp);
    var timediff = time2 - time1;
    analyticTotalSpenttime = convertMsToTime(timediff).toString();
    console.log("analyticTotalSpenttime "+analyticTotalSpenttime);
    if (this.pc != null) {
      console.log("Stop Window");
      this.pc.dispatchEvent(new CustomEvent('sendAnalytic', { detail: { connectionId: this.connectionId,dateTimeId:analyticDatetime, sku: SkuCode, avatarCode:AvatarCode,  ipv4:analyticIps, totalTimeSpent:analyticTotalSpenttime} }));
     // this.pc.dispatchEvent(new CustomEvent('sendCustomEvent', { detail: { connectionId: this.connectionId } }));

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

