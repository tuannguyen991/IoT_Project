# sudo chmod a+rw /dev/ttyACM0
import paho.mqtt.client as mqttclient
import time
import json
import math
import random
import serial.tools.list_ports

print("Hello ThingBoard")

BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883
THINGS_BOARD_ACCESS_TOKEN = "5pM1mSyz1RKlU7FkHWJC"
mess = ""

def getPort():
    ports = serial.tools.list_ports.comports()
    for i in range(len(ports)):
        print(str(ports[i]))
    N = len(ports)
    commPort = "None"
    for i in range(0, N):
        port = ports[i]
        strPort = str(port)
        print(strPort)
        if "BBC micro:bit CMSIS-DAP" in strPort:
            splitPort = strPort.split(" ")
            commPort = splitPort[0]
    return commPort

bbc_port = getPort()
if bbc_port != "None":
    ser = serial.Serial(port=bbc_port, baudrate=115200)

def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    led_data = {}
    pump_data = {}
    cmd = -1
    #0 : led1 off, 1 : led1 on, 2 : pump1 off, 3 : pump1 on
    #4 : led2 off, 5 : led2 on, 6 : pump2 off, 7 : pump2 on
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setLED":
            led_data['valueLED'] = jsonobj['params']
            if(jsonobj['params']):
                cmd = 1
            else:
                cmd = 0
            client.publish('v1/devices/me/attributes', json.dumps(led_data), 1)
        if jsonobj['method'] == "setPUMP":
            pump_data['valuePUMP'] = jsonobj['params']
            if(jsonobj['params']):
                cmd = 3
            else:
                cmd = 2
            client.publish('v1/devices/me/attributes', json.dumps(pump_data), 1)
        if jsonobj['method'] == "setLED2":
            led_data['valueLED2'] = jsonobj['params']
            if(jsonobj['params']):
                cmd = 5
            else:
                cmd = 4
            client.publish('v1/devices/me/attributes', json.dumps(led_data), 1)
        if jsonobj['method'] == "setPUMP2":
            pump_data['valuePUMP2'] = jsonobj['params']
            if(jsonobj['params']):
                cmd = 7
            else:
                cmd = 6
            client.publish('v1/devices/me/attributes', json.dumps(pump_data), 1)
        print(cmd)
    except:
        pass

    if bbc_port != "None":
        ser.write((str(cmd) + "#").encode())


def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Thingsboard connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")


client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(THINGS_BOARD_ACCESS_TOKEN)

client.on_connect = connected
client.connect(BROKER_ADDRESS, 1883)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message

def processData(data):
    data = data.replace("!", "")
    data = data.replace("#", "")
    splitData = data.split(":")
    print("Receive from sensor: ", splitData)
    #TODO: Add your source code to publish data to the server
    if len(splitData) != 3:
        print('line 128', data)
        return
    if splitData[0] == "1":
        if splitData[1] == "TEMP":
            client.publish("v1/devices/me/telemetry",  json.dumps({'temperature': splitData[2]}), 1)
        elif splitData[1] == "LIGHT":
            client.publish("v1/devices/me/telemetry",  json.dumps({'light': splitData[2]}), 1)
    if splitData[0] == "2":
        if splitData[1] == "TEMP":
            client.publish("v1/devices/me/telemetry",  json.dumps({'temperature2': splitData[2]}), 1)
        elif splitData[1] == "LIGHT":
            client.publish("v1/devices/me/telemetry",  json.dumps({'light2': splitData[2]}), 1)

def readSerial():
    bytesToRead = ser.inWaiting()
    if (bytesToRead > 0):
        global mess
        mess = mess + ser.read(bytesToRead).decode("UTF-8")
        while ("#" in mess) and ("!" in mess):
            start = mess.find("!")
            end = mess.find("#")
            processData(mess[start:end + 1])
            if (end == len(mess)):
                mess = ""
            else:
                mess = mess[end+1:]

temp = 30
light = 2
temp2 = 35
light2 = 10

while True:
    if bbc_port != "None":
        readSerial()
    collect_data = {'temperature': temp, 'light': light, 'temperature2': temp2, 'light2': light2}
    temp += 1
    light += 1
    temp2 += 1
    light2 += 1
    client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    time.sleep(5)
