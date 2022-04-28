using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace ChuongGa
{
    public class ChuongGaManager : MonoBehaviour
    {
        [SerializeField]
        private Text station_name;

        [SerializeField]
        private ChuongGaMqtt client;

        [SerializeField]
        private CanvasGroup _canvasLayer1;
        [SerializeField]
        private Text temperature;
        [SerializeField]
        private Text humidity;
        

        [SerializeField]
        private Toggle PumpControl;
        [SerializeField]
        private Text PumpStatus;
        [SerializeField]
        private CanvasGroup status_pump_on;
        [SerializeField]
        private CanvasGroup status_pump_off;

        [SerializeField]
        private Toggle LedControl;
        [SerializeField]
        private Text LedStatus;
        [SerializeField]
        private CanvasGroup status_led_on;
        [SerializeField]
        private CanvasGroup status_led_off;

        private Button _btn_config;
        /// <summary>
        /// Layer 2 elements
        /// </summary>
        /*[SerializeField]
        private CanvasGroup _canvasLayer2;
        [SerializeField]
        private InputField _input_min_tempe;
        [SerializeField]
        private InputField _input_max_tempe;
        [SerializeField]
        private Toggle ModeAuto;*/

        /// <summary>
        /// Layer 0 elements
        /// </summary>
        [SerializeField]
        private CanvasGroup _canvasLayer0;
        [SerializeField]
        private InputField _broker_url;
        [SerializeField]
        private InputField _username;
        [SerializeField]
        private InputField _password;
        [SerializeField]
        private Button _btn_connect;
        [SerializeField]
        public Text _notification;
        [SerializeField]
        public CanvasGroup _notification_status;


        /// <summary> 
        /// General elements
        /// </summary>
        //[SerializeField]
        //private GameObject Btn_Quit;

        private Tween twenFade;

        private bool device_status = false;
        public bool connected = false;


        public void Update_Status(Status_Data _status_data)
        {
            temperature.text = (_status_data.temperature) + "°C";
            humidity.text = (_status_data.humidity) + "%";
        }


        public void Update_Led(Led_Data _led_data)
        {
            if (_led_data.status == "ON")
            {
                LedControl.isOn = true;
                SwitchLed(true);
            }
            else
            {
                LedControl.isOn = false;
                SwitchLed(false);
            }
        }

        public void Update_Pump(Pump_Data _pump_data)
        {
            if (_pump_data.status == "ON")
            {
                PumpControl.isOn = true;
                SwitchPump(true);
            }
            else
            {
                PumpControl.isOn = false;
                SwitchPump(false);
            }
        }

        public void OnClickPump()
        {
            SwitchPump(PumpControl.isOn);
            client.PublishPump(PumpControl.isOn);
        }

        public void OnClickLed()
        {
            SwitchLed(LedControl.isOn);
            client.PublishLed(LedControl.isOn);
        }

        public void Fade(CanvasGroup _canvas, float endValue, float duration, TweenCallback onFinish)
        {
            if (twenFade != null)
            {
                twenFade.Kill(false);
            }

            twenFade = _canvas.DOFade(endValue, duration);
            twenFade.onComplete += onFinish;
        }

        public void FadeIn(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 1f, duration, () =>
            {
                _canvas.interactable = true;
                _canvas.blocksRaycasts = true;
            });
        }

        public void FadeOut(CanvasGroup _canvas, float duration)
        {
            Fade(_canvas, 0f, duration, () =>
            {
                _canvas.interactable = false;
                _canvas.blocksRaycasts = false;
            });
        }




        IEnumerator _IESwitchLayer()
        {
            if (_canvasLayer0.interactable == true)
            {
                FadeOut(_canvasLayer0, 0f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_canvasLayer1, 0f);
            }
            else
            {
                FadeOut(_canvasLayer1, 0f);
                yield return new WaitForSeconds(0.5f);
                FadeIn(_canvasLayer0, 0f);
            }
        }

        public void SwitchLayer()
        {
            StartCoroutine(_IESwitchLayer());
        }

        IEnumerator _IESwitchPump(bool isOn)
        {
            if (isOn)
            {
                FadeOut(status_pump_off, 0f);
                yield return new WaitForSeconds(0f);
                FadeIn(status_pump_on, 0f);
            }
            else
            {
                FadeOut(status_pump_on, 0f);
                yield return new WaitForSeconds(0f);
                FadeIn(status_pump_off, 0f);
            }
        }

        IEnumerator _IESwitchLed(bool isOn)
        {
            if (isOn)
            {
                FadeOut(status_led_off, 0f);
                yield return new WaitForSeconds(0f);
                FadeIn(status_led_on, 0f);
            }
            else
            {
                FadeOut(status_led_on, 0f);
                yield return new WaitForSeconds(0f);
                FadeIn(status_led_off, 0f);
            }
        }

        public void SwitchPump(bool isOn)
        {
            StartCoroutine(_IESwitchPump(isOn));

        }

        public void SwitchLed(bool isOn)
        {
            StartCoroutine(_IESwitchLed(isOn));
        }

        public void VerifyInputs()
        {
            if (_broker_url.text == "mqttserver.tk" && _username.text == "bkiot" && _password.text == "12345678")
            {
                connected = true;
            }
        }
        

    }
}