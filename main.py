print("IoT Gateway")

import paho.mqtt.client as mqttclient
import time
import json
import random

BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883 # default port of mqtt protocol
THINGS_BOARD_ACCESS_TOKEN = "5pM1mSyz1RKlU7FkHWJC"

def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")

def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    temp_data = {'value': True}
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setValue":
            temp_data['value'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    except:
        pass

def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Thingsboard connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")

def getRandom(default, step):
    return default + random.uniform(-1, 1) * step

client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(THINGS_BOARD_ACCESS_TOKEN)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message

tempDefault = 25   # default value of temperature
humiDefault = 50   # default value of humidity

tempStep = 5
humiStep = 30

counter = 0
while True:
    temp = getRandom(tempDefault, tempStep)
    humi = getRandom(humiDefault, humiStep)
    collect_data = {'temperature': temp, 'humidity': humi}
    client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    time.sleep(10)
