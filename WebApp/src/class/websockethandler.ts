import Offer from './offer';
import Answer from './answer';
import Candidate from './candidate';
import { config, DynamoDB } from "aws-sdk";
var AWS = require("aws-sdk");
let awsConfig = {
    "region": "ap-south-1",
    "accessKeyId": "AKIAQUOTQMLFYPDZ2KRQ", "secretAccessKey": "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"
};
AWS.config.update(awsConfig);

let docClient = new AWS.DynamoDB.DocumentClient();
var isfromLocal = true;
export function modify (ipport) {
    // if(isfromLocal){
    //   return;
    // }
    ipport = "13.234.185.150:8558";
    var params = {
        TableName: "Servers-inGnious-Demo",
        Key: { "ip": ipport },
        UpdateExpression: "set instance_status = :bystatus",
        ConditionExpression: 'attribute_exists(ip)',
        ExpressionAttributeValues: {
            ":bystatus": "Ready"
        },
        ReturnValues: "ALL_NEW"

    };
    docClient.update(params, function (err, data) {

        if (err) {
            console.log("users::update::error - " + JSON.stringify(err, null, 2));
        } else {
            console.log("users::update::success "+JSON.stringify(data) );
        }
    });
}
 function SetAnalytic (dateTimeId,avatarCode,sku,totalTimeSpent,ipv4) {
    
  var params = {
      TableName: "AnalyticData",
      Key: { "DateTimeId": dateTimeId },
      UpdateExpression: "set avatarCode = :avatarCode, sku = :sku, totalTimeSpent = :totalTimeSpent, ipv4 = :ipv4 ",
      ExpressionAttributeValues: {
          ":avatarCode": avatarCode,
          ":sku": sku,
          ":totalTimeSpent": totalTimeSpent,
          ":ipv4": ready

      },
      ReturnValues: "UPDATED_NEW"

  };
  docClient.update(params, function (err, data) {

      if (err) {
          console.log("users::update::error - " + JSON.stringify(err, null, 2));
      } else {
          console.log("users::update::success "+JSON.stringify(data) );
      }
  });
}
let isPrivate: boolean;
console.log("WebSocket handler");

// [{sessonId:[connectionId,...]}]
const clients: Map<WebSocket, Set<string>> = new Map<WebSocket, Set<string>>();

// [{connectionId:[sessionId1, sessionId2]}]
const connectionPair: Map<string, [WebSocket, WebSocket]> = new Map<string, [WebSocket, WebSocket]>();

function getOrCreateConnectionIds(session: WebSocket): Set<string> {
  let connectionIds = null;
  if (!clients.has(session)) {
    connectionIds = new Set<string>();
    clients.set(session, connectionIds);
  }
  connectionIds = clients.get(session);
  return connectionIds;
}

function reset(mode: string): void {
  isPrivate = mode == "private";
}

function add(ws: WebSocket): void {
  console.log("WebSocket connection add");

  clients.set(ws, new Set<string>());
}

function remove(ws: WebSocket): void {
  console.log("WebSocket connection remove");
   
  const connectionIds = clients.get(ws);
  connectionIds.forEach(connectionId => {

    const pair = connectionPair.get(connectionId);
    if (pair) {
      const otherSessionWs = pair[0] == ws ? pair[1] : pair[0];
      if (otherSessionWs) {
        otherSessionWs.send(JSON.stringify({ type: "disconnect", connectionId: connectionId }));
      }
    }
    connectionPair.delete(connectionId);
  });

  clients.delete(ws);
 
 

}

function onConnect(ws: WebSocket, connectionId: string): void {
  console.log("WebSocket onConnect");
  let polite = true;
  if (isPrivate) {
    if (connectionPair.has(connectionId)) {
      const pair = connectionPair.get(connectionId);

      if (pair[0] != null && pair[1] != null) {
        ws.send(JSON.stringify({ type: "error", message: `${connectionId}: This connection id is already used.` }));
        return;
      } else if (pair[0] != null) {
        connectionPair.set(connectionId, [pair[0], ws]);
      }
    } else {
      connectionPair.set(connectionId, [ws, null]);
      polite = false;
    }
  }

  const connectionIds = getOrCreateConnectionIds(ws);
  connectionIds.add(connectionId);
  ws.send(JSON.stringify({ type: "connect", connectionId: connectionId, polite: polite }));
}

function onDisconnect(ws: WebSocket, connectionId: string): void {
  console.log("WebSocket onDisconnect");

  const connectionIds = clients.get(ws);
  connectionIds.delete(connectionId);

  if (connectionPair.has(connectionId)) {
    const pair = connectionPair.get(connectionId);
    const otherSessionWs = pair[0] == ws ? pair[1] : pair[0];
    if (otherSessionWs) {
      otherSessionWs.send(JSON.stringify({ type: "disconnect", connectionId: connectionId }));
    }
  }
  connectionPair.delete(connectionId);
  ws.send(JSON.stringify({ type: "disconnect", connectionId: connectionId }));
}

function onOffer(ws: WebSocket, message: any): void {
  const connectionId = message.connectionId as string;
  const newOffer = new Offer(message.sdp, Date.now(), false);

  if (isPrivate) {
    if (connectionPair.has(connectionId)) {
      const pair = connectionPair.get(connectionId);
      const otherSessionWs = pair[0] == ws ? pair[1] : pair[0];
      if (otherSessionWs) {
        newOffer.polite = true;
        otherSessionWs.send(JSON.stringify({ from: connectionId, to: "", type: "offer", data: newOffer }));
      }
    }
    return;
  }

  connectionPair.set(connectionId, [ws, null]);
  clients.forEach((_v, k) => {
    if (k == ws) {
      return;
    }
    k.send(JSON.stringify({ from: connectionId, to: "", type: "offer", data: newOffer }));
  });
}

function onAnswer(ws: WebSocket, message: any): void {
  console.log("onAnswer called")
  const connectionId = message.connectionId as string;
  const connectionIds = getOrCreateConnectionIds(ws);
  connectionIds.add(connectionId);
  const newAnswer = new Answer(message.sdp, Date.now());

  if (!connectionPair.has(connectionId)) {
    return;
  }

  const pair = connectionPair.get(connectionId);
  const otherSessionWs = pair[0] == ws ? pair[1] : pair[0];

  if (!isPrivate) {
    connectionPair.set(connectionId, [otherSessionWs, ws]);
  }

  otherSessionWs.send(JSON.stringify({ from: connectionId, to: "", type: "answer", data: newAnswer }));
}

function onCandidate(ws: WebSocket, message: any): void {
  const connectionId = message.connectionId;
  const candidate = new Candidate(message.candidate, message.sdpMLineIndex, message.sdpMid, Date.now());

  if (isPrivate) {
    if (connectionPair.has(connectionId)) {
      const pair = connectionPair.get(connectionId);
      const otherSessionWs = pair[0] == ws ? pair[1] : pair[0];
      if (otherSessionWs) {
        otherSessionWs.send(JSON.stringify({ from: connectionId, to: "", type: "candidate", data: candidate }));
      }
    }
    return;
  }

  clients.forEach((_v, k) => {
    if (k === ws) {
      return;
    }
    k.send(JSON.stringify({ from: connectionId, to: "", type: "candidate", data: candidate }));
  });
}
function onAnalytic(ws: WebSocket, message: any): void {
  const sku = message.sku;
  const avatarCode = message.avatarCode;
  const dateTimeId = message.dateTimeId;
  const ipv4 = message.ipv4;
  const totalTimeSpent = message.totalTimeSpent;
  console.log("dateTimeId "+dateTimeId);
  console.log("ipv4 "+ipv4);
  console.log("totalTimeSpent "+totalTimeSpent);
  console.log("avatarCode "+avatarCode);
  console.log("sku "+sku);
  SetAnalytic(dateTimeId,avatarCode,sku,totalTimeSpent,ipv4);
}


function makeStatusReady(): void {
 // console.log("fun() start");
  //exec('/Users/hetalchirag/InGnious/VirtuFit_RenderStreaming-HDRP/test.app/Contents/MacOS/VirtuFit_HDRP_RenderStreaming', function(err, data) {  
    //   console.log(err)
      // console.log(data.toString());                       
   //});  
   modify(Ipport);
}

let Ipport;
export function SetIpPort(ipport){
  Ipport = ipport;
}
export { reset, add, remove, onConnect, onDisconnect, onOffer, onAnswer, onCandidate, makeStatusReady, onAnalytic };
