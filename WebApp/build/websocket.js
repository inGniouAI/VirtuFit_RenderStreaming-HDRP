"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var websocket = require("ws");
var handler = require("./class/websockethandler");
var WSSignaling = /** @class */ (function () {
    function WSSignaling(server, mode) {
        var _this = this;
        this.server = server;
        this.wss = new websocket.Server({ server: server });
        handler.reset(mode);
        this.wss.on('connection', function (ws) {
            console.log("connection add");
            handler.add(ws);
            ws.onclose = function () {
                console.log("connection remove");
                handler.remove(ws);
            };
            ws.onmessage = function (event) {
                // type: connect, disconnect JSON Schema
                // connectionId: connect or disconnect connectionId
                // type: offer, answer, candidate JSON Schema
                // from: from connection id
                // to: to connection id
                // data: any message data structure
                var msg = JSON.parse(event.data);
                if (!msg || !_this) {
                    return;
                }
                console.log(msg);
                switch (msg.type) {
                    case "connect":
                        console.log("connect from websocket");
                        handler.onConnect(ws, msg.connectionId);
                        break;
                    case "disconnect":
                        console.log("disconnect from websocket");
                        handler.onDisconnect(ws, msg.connectionId);
                        break;
                    case "offer":
                        console.log("offer from websocket");
                        handler.onOffer(ws, msg.data);
                        break;
                    case "answer":
                        console.log("answer from websocket");
                        handler.onAnswer(ws, msg.data);
                        break;
                    case "candidate":
                        console.log("candidate from websocket");
                        handler.onCandidate(ws, msg.data);
                        break;
                    case "CustomEvent":
                        console.log("time to restart unity");
                        setTimeout(function () {
                            handler.onRestartUnityapp();
                        }, 2000);
                        //  handler.onConnect(ws, msg.connectionId);
                        break;
                    default:
                        break;
                }
            };
        });
    }
    return WSSignaling;
}());
exports.default = WSSignaling;
