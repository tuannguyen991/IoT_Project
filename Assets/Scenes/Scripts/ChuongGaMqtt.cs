using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace ChuongGa
{
    public class Status_Data
    {
        //public string project_id { get; set; }
        //public string project_name { get; set; }
        //public string station_id { get; set; }
        //public string station_name { get; set; }
        //public string longitude { get; set; }
        //public string latitude { get; set; }
        //public string volt_battery { get; set; }
        //public string volt_solar { get; set; }
        //public List<data_ss> data_ss { get; set; }
        //public string device_status { get; set; }
        public float temperature { get; set; }
        public float humidity { get; set; }
        public int device_status { get; set; }
        //public Status_Data(float temp, float humidity, float min_temp, float max_temp, int access)
        //{
        //    this.temperature = temp;
        //    this.humidity = humidity;
        //    this.min_temperature = min_temp;
        //    this.max_temperature = max_temp;
        //    this.device_status = access;
        //}
    }

    public class data_ss
    {
        public string ss_name { get; set; }
        public string ss_unit { get; set; }
        public string ss_value { get; set; }
    }

    public class Pump_Data
    {
        public string device { get; set; }
        public string status { get; set; }
        public Pump_Data(string device, string status)
        {
            this.device = device;
            this.status = status;
        }
    }

    public class Led_Data
    {
        public string device { get; set; }
        public string status { get; set; }
        public Led_Data(string device, string status)
        {
            this.device = device;
            this.status = status;
        }
    }

    public class ChuongGaMqtt : M2MqttUnityClient
    {
        public InputField addressInputField;
        public InputField userInputField;
        public InputField pwdInputField;
        public Text text_display;
        public ChuongGaManager manager;

        public List<string> topics = new List<string>();


        public string msg_received_from_topic_status = "";
        public string msg_received_from_topic_led = "";
        public string msg_received_from_topic_pump = "";


        private List<string> eventMessages = new List<string>();
        [SerializeField]
        public Status_Data _status_data;
        [SerializeField]
        public Pump_Data _pump_data;
        [SerializeField]
        public Led_Data _led_data;

        


        public async void  PublishLed(bool isOn)
        {
            //_config_data = new Config_Data();
            //GetComponent<ChuongGaManager>().Update_Config_Value(_config_data);
            _led_data = new Led_Data("LED", isOn ? "ON" : "OFF");
            string msg_led = JsonConvert.SerializeObject(_led_data);
            client.Publish(topics[1], System.Text.Encoding.UTF8.GetBytes(msg_led), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("publish Led");
        }

        public async void PublishPump(bool isOn)
        {
            _pump_data = new Pump_Data("PUMP", isOn ? "ON" : "OFF");
            string msg_pump = JsonConvert.SerializeObject(_pump_data);
            client.Publish(topics[2], System.Text.Encoding.UTF8.GetBytes(msg_pump), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Debug.Log("publish Pump");
        }

        

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }


        protected override void OnConnecting()
        {
            base.OnConnecting();
        }

        protected override async void OnConnected()
        {
            base.OnConnected();

            SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {

            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                }
            }
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string topic in topics)
            {
                if (topic != "")
                {
                    client.Unsubscribe(new string[] { topic });
                }
            }

        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            //_connect_status = false;
            Debug.Log("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            Debug.Log("CONNECTION LOST!");
        }

        public void ConnectServer()
        {
            Debug.Log(GetComponent<ChuongGaManager>().connected);
            if (GetComponent<ChuongGaManager>().connected)
            {
                Debug.Log(GetComponent<ChuongGaManager>().connected);
                base.Start();
                GetComponent<ChuongGaManager>().SwitchLayer();
            }
            else
            {
                Debug.Log(GetComponent<ChuongGaManager>().connected);
                GetComponent<ChuongGaManager>()._notification.text = "Connect Fail";
                //GetComponent<ChuongGaManager>().FadeIn(GetComponent<ChuongGaManager>()._notification, 1f);
            }

            //if (addressInputField.text != "")
            //{
            //    this.brokerAddress = addressInputField.text;
            //}
            //else
            //{
            //    this.brokerAddress = "mqttserver.tk";
            //}

            //if (userInputField.text != "")
            //{
            //    this.mqttUserName = userInputField.text;
            //}
            //else
            //{
            //    this.mqttUserName = "bkiot";
            //}

            //if (pwdInputField.text != "")
            //{
            //    this.mqttPassword = pwdInputField.text;
            //}
            //else
            //{
            //    this.mqttPassword = "12345678";
            //}
            //this.brokerPort = 1883;
            //base.Connect();
        }


        protected override void Start()
        {
            //base.Start();
        }


        protected override async void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            //StoreMessage(msg);
            if (topic == topics[0])
                ProcessMessageStatus(msg);

            if (topic == topics[1])
                ProcessMessageLed(msg);

            if (topic == topics[2])
                ProcessMessagePump(msg);
        }

        private void ProcessMessageStatus(string msg)
        {
            _status_data = JsonConvert.DeserializeObject<Status_Data>(msg);
            msg_received_from_topic_status = msg;
            GetComponent<ChuongGaManager>().Update_Status(_status_data);
        }

        private void ProcessMessageLed(string msg)
        {
            _led_data = JsonConvert.DeserializeObject<Led_Data>(msg);
            msg_received_from_topic_led = msg;
            GetComponent<ChuongGaManager>().Update_Led(_led_data);
        }

        private void ProcessMessagePump(string msg)
        {
            _pump_data = JsonConvert.DeserializeObject<Pump_Data>(msg);
            msg_received_from_topic_pump = msg;
            GetComponent<ChuongGaManager>().Update_Pump(_pump_data);
        }

        private void OnDestroy()
        {
            Disconnect();
        }

    }
}