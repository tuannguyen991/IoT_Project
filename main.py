print("IoT Gateway")

import paho.mqtt.client as mqttclient
import time
import json
import random

from selenium import webdriver
from selenium.webdriver.common.by import By
import os

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

def getLocation():
    driver = webdriver.Chrome()
    driver.get("https://www.google.com/maps")

    string = '''
        function getLocation(callback) {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function(position) {
                    var myjson = {"latitude":position.coords.latitude, "longitude":position.coords.longitude};
                    var stringJson = JSON.stringify(myjson);
                    callback(stringJson);
                });
            }
        }
        
        getLocation(function(callback) {
            const para = document.createElement("p");
            para.innerHTML = callback;
            para.id = "location";
            document.body.appendChild(para);
        })
    '''

    driver.execute_script(string)
    time.sleep(2)
    res = driver.find_element(By.ID, "location")
    loc = res.text
    locDict = json.loads(loc)

    os.system("pkill chrome")
    return locDict["latitude"], locDict["longitude"]

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

latitude = 10.8231 
longitude = 106.6297

counter = 0
while True:
    temp = getRandom(tempDefault, tempStep)
    humi = getRandom(humiDefault, humiStep)
    latitude, longitude = getLocation()
    collect_data = {'temperature': temp, 'humidity': humi, 'latitude': latitude, 'longitude': longitude}
    client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    time.sleep(10)
