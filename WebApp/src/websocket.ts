import * as websocket from "ws";
import { Server } from 'http';
import * as handler from "./class/websockethandler"

export default class WSSignaling {
  server: Server;
  wss: websocket.Server;

  constructor(server: Server, mode: string) {
    this.server = server;
    this.wss = new websocket.Server({ server });
    handler.reset(mode);

    this.wss.on('connection', (ws: WebSocket) => {
      console.log("connection add");

      handler.add(ws);

      ws.onclose = (): void => {
        console.log("connection remove");
        handler.remove(ws);
      }

      ws.onmessage = (event: MessageEvent): void => {

        // type: connect, disconnect JSON Schema
        // connectionId: connect or disconnect connectionId

        // type: offer, answer, candidate JSON Schema
        // from: from connection id
        // to: to connection id
        // data: any message data structure

        const msg = JSON.parse(event.data);
        if (!msg || !this) {
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
             handler.makeStatusReady(); 
            break;
          case "analytic":
            console.log("Analytic");
            handler.onAnalytic(ws, msg.data);
            break; 
          case "openUnity" :
            console.log("onUnityappOpen");
            break;
          default:
            break;
        }
      };
    });
  }
}
