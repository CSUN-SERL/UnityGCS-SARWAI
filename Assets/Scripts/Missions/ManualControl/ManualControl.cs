﻿using FeedScreen.Experiment.Missions.Broadcasts.Events;
using LiveFeedScreen.ROSBridgeLib;
using LiveFeedScreen.ROSBridgeLib.geometry_msgs.geometry_msgs;
using LiveFeedScreen.ROSBridgeLib.std_msgs.std_msgs;
using UnityEngine;


namespace Missions.ManualControl
{
    public class ManualControl : MonoBehaviour
    {
        private static bool updateControl;


        private ROSBridgeWebSocketConnection _ros;


        private string _topic = "/cmd_vel";
        private bool _useJoysticks;


        private string FinalTopic { get; set; }

        private void OnEnable()
        {
            EventManager.ManualControlEnable += OnManualControlEnable;
            EventManager.ManualControlDisable += OnManualControlDisable;
        }

        private void OnManualControlEnable(object sender, IntEventArgs e)
        {
            //Debug.Log(e.intField);
            ActiveToggle(e.intField);
            updateControl = true;

            //TODO: start manual control
        }


        private void OnDisable()
        {
            EventManager.ManualControlEnable -= OnManualControlEnable;
            EventManager.ManualControlDisable -= OnManualControlDisable;
        }

        private void OnManualControlDisable(object sender, IntEventArgs e)
        {
            //Debug.Log(e.intField);
            ActiveToggle(e.intField);
            updateControl = false;


            //TODO: stop manual control
        }


        // The critical thing here is to define our subscribers, publishers and service response handlers.
        private void Start()
        {
            _useJoysticks = Input.GetJoystickNames().Length > 0;

            // ros will be a node with said connection below... To our AWS server.
            _ros = new ROSBridgeWebSocketConnection("ws:ubuntu@13.57.99.200",
                9090);

            // Gives a live connection to ROS via ROSBridge.
            _ros.Connect();
        }

        // Extremely important to disconnect from ROS. OTherwise packets continue to flow.
        private void OnApplicationQuit()
        {
            if (_ros != null)
                _ros.Disconnect();
        }

        // Update is called once per frame in Unity. We use the joystick or cursor keys to generate teleoperational commands
        // that are sent to the ROS world, which drives the robot which ...
        private void Update()
        {
            if (updateControl)
            {
                // Instantiates variables with keyboad input (Lines 44 - 62).
                float _dx, _dy;

                if (_useJoysticks)
                {
                    _dx = Input.GetAxis("Joy0X");
                    _dy = Input.GetAxis("Joy0Y");
                }
                else
                {
                    _dx = Input.GetAxis("Horizontal");
                    _dy = Input.GetAxis("Vertical");
                }

                // Multiplying _dy or _dx by a larger value, increases "speed".
                // Linear is responsibile for forward and backward movment.
                var linear = _dy * 3.0f;
                //angular is responsible for rotaion.
                var angular = -_dx * 2.0f;

                // Create a ROS Twist message from the keyboard input. This input/twist message, creates the data that will in turn move the 
                // bot on the ground.
                var msg = new TwistMsg(new Vector3Msg(linear, 0.0, 0.0),
                    new Vector3Msg(0.0, 0.0, angular));

                ///////\\\\\\\\\ Need to call func to get appropriate topic
                // name for the correct selected bot. This is for testing of 
                // the 4 bot environment!!!!!!

                // Publishes the TwistMsg values over to the /cmd_vel topic in ROS.
                _ros.Publish("/cmd_vel", msg);

                /*var msg2 = new Int32Msg(2);
                _ros.Publish("/int", msg2);*/

                _ros.Publish(FinalTopic,
                    msg); /////////\\\\\\\\\\ this is for testing of the 4 bots 
                // environment!!!	

                _ros.Render();
            }
        }

        private void ActiveToggle(int RobotId)
        {
            _topic = "/robot{}/cmd_vel";
            FinalTopic = string.Format(_topic, RobotId);
        }
    }
}