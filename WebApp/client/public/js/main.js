import * as Config from "./config.js";
import { VirtualFitReceiver } from "./virtufit.js";
import { registerGamepadEvents, registerKeyboardEvents, registerMouseEvents, sendClickEvent, sendInputTextEvent , sendEnvChangeId, sendNecklaceChangeId, sendBanglesChangeId} from "./register-events.js";
import { getServerConfig } from "./config.js";
var AvtarCodeVal;
var InputSettings;

OnLoad();
setup();

let playButton;
let virtuFitReceiver;
let useWebSocket;

window.document.oncontextmenu = function () {
  return false;     // cancel default menu
};

window.addEventListener('resize', function () {
  virtuFitReceiver.resizeVideo();
}, true);

window.addEventListener('beforeunload', async () => {
  await virtuFitReceiver.stop();
}, true);

async function setup() {
  console.log("setup called");

  const res = await getServerConfig();
  useWebSocket = res.useWebSocket;
  showWarningIfNeeded(res.startupMode);
  showPlayButton();
}

function showWarningIfNeeded(startupMode) {
  const warningDiv = document.getElementById("warning");
  if (startupMode == "private") {
    warningDiv.innerHTML = "<h4>Warning</h4> This sample is not working on Private Mode.";
    warningDiv.hidden = false;
  }
}

function showPlayButton() {
  if (!document.getElementById('playButton')) {
    let elementPlayButton = document.createElement('img');
    elementPlayButton.id = 'playButton';
    elementPlayButton.src = 'images/Play.png';
    elementPlayButton.alt = 'Start Streaming';
    playButton = document.getElementById('player').appendChild(elementPlayButton);
    playButton.addEventListener('click', onClickPlayButton);
  }
}

function onClickPlayButton() {

  playButton.style.display = 'none';

  const playerDiv = document.getElementById('player');

  // add video player
  const elementVideo = document.createElement('video');
  elementVideo.id = 'Video';
  elementVideo.style.touchAction = 'none';
  playerDiv.appendChild(elementVideo);

  // add video thumbnail
  // const elementVideoThumb = document.createElement('video');
  // elementVideoThumb.id = 'VideoThumbnail';
  // elementVideoThumb.style.touchAction = 'none';
  // playerDiv.appendChild(elementVideoThumb);

  setupVideoPlayer(elementVideo).then(value => virtuFitReceiver = value);
// add inputtext
// const elementinputText = document.createElement('INPUT');
// elementinputText.id = "SKUInputText";
// elementinputText.innerHTML = "SKU";
// playerDiv.appendChild(elementinputText);


// add Start button
const elementStartButton = document.createElement('button');
elementStartButton.id = "startButton";
elementStartButton.innerHTML = "Settings";
playerDiv.appendChild(elementStartButton);
elementStartButton.addEventListener("click", function () {
   openNav();
});

// add SKU button
const elementSKUButton = document.createElement('button');
elementSKUButton.id = "SKUButton";
elementSKUButton.innerHTML = "SKU";
playerDiv.appendChild(elementSKUButton);
elementSKUButton.addEventListener("click", function () {
  sendInputTextEvent(virtuFitReceiver, AvtarCodeVal);
 // openNav();
});
// create Env buttons
const elementScrollEnv = document.getElementById('ScrollEnv');

for(let i =0; i<2;i++){
  const Env = document.createElement('button');
  Env.id = "Env"+i;
  Env.innerHTML = "Env"+i;
  elementScrollEnv.appendChild(Env);
  InputSettings = i;
  Env.addEventListener("click", function () {
    sendEnvChangeId(virtuFitReceiver, InputSettings);
    closeNav();
  });
}
// create Necklace buttons

const elementScrollNecklace = document.getElementById('ScrollNecklace');

for(let i =0; i<2;i++){
  const Necklace = document.createElement('button');
  Necklace.id = "Neck"+i;
  Necklace.innerHTML = "Neck"+i;
  elementScrollNecklace.appendChild(Necklace);
  InputSettings = i;
  Necklace.addEventListener("click", function () {
    sendNecklaceChangeId(virtuFitReceiver, InputSettings);
    closeNav();
  });
}
// create bangle buttons

const elementScrollBangle = document.getElementById('ScrollBangle');

for(let i =0; i<2;i++){
  const Bangle = document.createElement('button');
  Bangle.id = "Bangle"+i;
  Bangle.innerHTML = "Bangle"+i;
  elementScrollBangle.appendChild(Bangle);
  InputSettings = i;
  Bangle.addEventListener("click", function () {
    sendBanglesChangeId(virtuFitReceiver, InputSettings);
    closeNav();
  });
}

  // add fullscreen button
  const elementFullscreenButton = document.createElement('img');
  elementFullscreenButton.id = 'fullscreenButton';
  elementFullscreenButton.src = 'images/FullScreen.png';
  playerDiv.appendChild(elementFullscreenButton);
  elementFullscreenButton.addEventListener("click", function () {
    if (!document.fullscreenElement || !document.webkitFullscreenElement) {
      if (document.documentElement.requestFullscreen) {
        document.documentElement.requestFullscreen();
      }
      else if (document.documentElement.webkitRequestFullscreen) {
        document.documentElement.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
      } else {
        if (playerDiv.style.position == "absolute") {
          playerDiv.style.position = "relative";
        } else {
          playerDiv.style.position = "absolute";
        }
      }
    }
  });
  document.addEventListener('webkitfullscreenchange', onFullscreenChange);
  document.addEventListener('fullscreenchange', onFullscreenChange);

  function onFullscreenChange() {
    if (document.webkitFullscreenElement || document.fullscreenElement) {
      playerDiv.style.position = "absolute";
      elementFullscreenButton.style.display = 'none';
    }
    else {
      playerDiv.style.position = "relative";
      elementFullscreenButton.style.display = 'block';
    }
  }
}

async function setupVideoPlayer(elements) {
  const videoPlayer = new VirtualFitReceiver(elements);
  await videoPlayer.setupConnection(useWebSocket);

  videoPlayer.ondisconnect = onDisconnect;
  videoPlayer.onconnect = onConnect;

  registerGamepadEvents(videoPlayer);
  registerKeyboardEvents(videoPlayer);
  registerMouseEvents(videoPlayer, elements);
  return videoPlayer;
}

function onDisconnect() {
  const playerDiv = document.getElementById('player');
  clearChildren(playerDiv);
  virtuFitReceiver.stop();
  virtuFitReceiver = null;
  showPlayButton();
}
function onConnect() {
  console.log("onConnect called");
  setTimeout(function(){
    sendInputTextEvent(virtuFitReceiver, AvtarCodeVal);
  }, 1000); 
  console.log("AvtarCodeVal "+AvtarCodeVal);
 
}

function clearChildren(element) {
  while (element.firstChild) {
    element.removeChild(element.firstChild);
  }
}

function ShowSetting(){

}

function openNav() {
  document.getElementById("myNav").style.width = "100%";
  }
  
  function closeNav() {
  document.getElementById("myNav").style.width = "0%";
  }
  function OnLoad(){
    var queryString = new Array();
    window.onload = function () {
      console.log("onload called");
  
        if (queryString.length == 0) {
            if (window.location.search.split('?').length > 1) {
                var params = window.location.search.split('?')[1].split('&');
                for (var i = 0; i < params.length; i++) {
                    var key = params[i].split('=')[0];
                    var value = decodeURIComponent(params[i].split('=')[1]);
                    queryString[key] = value;
                }
            }
        }
        if (queryString["AvatarCode"] != null && queryString["Sku"] != null) {
            var val1 = "<h3>After taking multiple parameters from the previous page, we have the following information that you need:</h3><br /><br />";
            val1 += "<b>AvatarCode:</b> " + queryString["AvatarCode"] + "<br> <b>Sku:</b> " + queryString["Sku"];
            document.getElementById("lblredirect").innerHTML = val1; 
  
            var val = queryString["AvatarCode"];
            AvtarCodeVal = val;
         
  
        }
    };
  }
  