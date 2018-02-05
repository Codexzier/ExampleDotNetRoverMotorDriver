using System;

using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT;

namespace ExampleRoverMotorDriver
{
    /// <summary>
    /// Motor driver to control the rover 5 chassis.
    /// </summary>
    public class RoverMotorDriver
    {
        /// <summary>
        /// Pin connected to ST_CP of 74HC595
        /// </summary>
        private OutputPort _latch;

        /// <summary>
        /// Pin connected to SH_CP of 74HC595
        /// </summary>
        private OutputPort _clock;

        /// <summary>
        /// Pin connected to DS of 74HC595
        /// </summary>
        private OutputPort _data;

        /// <summary>
        /// enable the Shiftregister outputs
        /// </summary>
        private OutputPort _regEn;

        /// <summary>
        /// Output value to M1
        /// </summary>
        private PWM _outputM1;

        /// <summary>
        /// Output value to M2
        /// </summary>
        private PWM _outputM2;

        /// <summary>
        /// Output value to M3
        /// </summary>
        private PWM _outputM3;

        /// <summary>
        /// Output value to M4
        /// </summary>
        private PWM _outputM4;


        private const int _outputM1_A = 4;           
        private const int _outputM1_B = 8;           
        private const int _outputM2_A = 2;           
        private const int _outputM2_B = 16;          
        private const int _outputM3_A = 32;           
        private const int _outputM3_B = 128;          
        private const int _outputM4_A = 1;            
        private const int _outputM4_B = 64;           

        /// <summary>
        /// Initalize all outputs 
        /// </summary>
        public RoverMotorDriver()
        {
            Debug.Print("Init Shiftregister");
            this._latch = new OutputPort(Pins.GPIO_PIN_D12, false);
            this._clock = new OutputPort(Pins.GPIO_PIN_D4, false);
            this._data = new OutputPort(Pins.GPIO_PIN_D8, false);
            this._regEn = new OutputPort(Pins.GPIO_PIN_D7, false);


            Debug.Print("PWM Output");
            this._outputM1 = new PWM(PWMChannels.PWM_PIN_D11, 200d, .1d, false);
            this._outputM1.Duration = 1;
            this._outputM1.Start();

            this._outputM2 = new PWM(PWMChannels.PWM_PIN_D3, 200d, .1d, false);
            this._outputM2.Duration = 1;
            this._outputM2.Start();

            this._outputM3 = new PWM(PWMChannels.PWM_PIN_D6, 200d, .1d, false);
            this._outputM3.Duration = 1;
            this._outputM3.Start();

            this._outputM4 = new PWM(PWMChannels.PWM_PIN_D5, 200d, .1d, false);
            this._outputM4.Duration = 1;
            this._outputM4.Start();
        }

        /// <summary>
        /// Enable or disable the motorshield
        /// </summary>
        /// <param name="setOn">Take the motor shield.</param>
        public void SetMotorShieldOn(bool setOn)
        {
            this._regEn.Write(!setOn);
        }

        /// <summary>
        /// set all pwm to 0 and disable the motorshield
        /// </summary>
        public void MotorStop()
        {
            this.SetRunMotors(0, 0, 0);
            this.SetMotorShieldOn(false);
        }

        /// <summary>
        /// Motor drives forward. 
        /// Set get value and shift all register to one value and set the motor outputs.
        /// </summary>
        /// <param name="speedValueLeft">left side pwm value output</param>
        /// <param name="speedValueRight">right side pwm value output</param>
        public void SetForeward(int speedValueLeft, int speedValueRight)
        {
            int motorsOn = _outputM1_B | _outputM2_B | _outputM3_A | _outputM4_A;
            this.SetRunMotors(motorsOn, speedValueLeft, speedValueRight);

            this.SetMotorShieldOn(true);
        }

        /// <summary>
        /// Motor drives backwards. 
        /// Set get value and shift all register to one value and set the motor outputs
        /// </summary>
        /// <param name="speedValueLeft">left side pwm value output</param>
        /// <param name="speedValueRight">right side pwm value output</param>
        public void SetBackward(int speedValueLeft, int speedValueRight)
        {
            int motorsOn = _outputM1_A | _outputM2_A | _outputM3_B | _outputM4_B;
            this.SetRunMotors(motorsOn, speedValueLeft, speedValueRight);

            this.SetMotorShieldOn(true);
        }

        /// <summary>
        /// Set left motors forward and right motors backwards
        /// </summary>
        /// <param name="speedValue">pwm value for all motors</param>
        public void SetTurnLeft(int speedValue)
        {
            int motorsOn = _outputM1_B | _outputM2_B | _outputM3_B | _outputM4_B;
            SetRunMotors(motorsOn, speedValue, speedValue);
            SetMotorShieldOn(true);
        }

        /// <summary>
        /// Set left motors backwards and right motors forwards.
        /// </summary>
        /// <param name="speedValue">pwm value for all motors</param>
        public void SetTurnRight(int speedValue)
        {
            int motorsOn = _outputM1_A | _outputM2_A | _outputM3_A | _outputM4_A;
            SetRunMotors(motorsOn, speedValue, speedValue);
            SetMotorShieldOn(true);
        }

        /// <summary>
        /// Set target output and level output on motor driver
        /// </summary>
        /// <param name="motorsOn">shift register value to take on the outputs</param>
        /// <param name="speedValueLeft">left side pwm value output</param>
        /// <param name="speedValueRight">right side pwm value output</param>
        private void SetRunMotors(int motorsOn, int speedValueLeft, int speedValueRight)
        {
            this.SetOutputValue(speedValueLeft, speedValueRight);

            this.RegisterWrite(motorsOn);
        }

        /// <summary>
        /// Set level output on motor driver
        /// </summary>
        /// <param name="speedValueLeft">left side pwm value output</param>
        /// <param name="speedValueRight">right side pwm value output</param>
        private void SetOutputValue(int speedValueLeft, int speedValueRight)
        {
            this._outputM1.Duration = this.GetLimitValue(speedValueLeft);
            this._outputM2.Duration = this.GetLimitValue(speedValueLeft);
            this._outputM3.Duration = this.GetLimitValue(speedValueRight);
            this._outputM4.Duration = this.GetLimitValue(speedValueRight);
        }

        /// <summary>
        /// Set limit the value if it under 0 or over than 1000.
        /// </summary>
        /// <param name="durationValue">Duration to limited</param>
        /// <returns>Return a Value between 1000 to 2000</returns>
        private uint GetLimitValue(int durationValue)
        {
            if(durationValue > 1000)
            {
                durationValue = 1000;
            }

            if(durationValue < 0)
            {
                durationValue = 0;
            }

            return 1000 + (uint)durationValue;
        }

        /// <summary>
        /// Write the value to shiftregister to take on the target outputs.
        /// </summary>
        /// <param name="motors">Set value to setup outputs</param>
        private void RegisterWrite(int motors)
        {
            this._latch.Write(false);

            for (int bit = 0; bit < 8; bit++)
            {
                int target = motors & 0x80;
                motors = motors << 1;

                this._data.Write(target == 128);

                this._clock.Write(true);
                this.SleepWhile(5);
                this._clock.Write(false);
            }

            this._latch.Write(true);
        }

        /// <summary>
        /// Dient als ersatz zu Thread.Sleep
        /// </summary>
        /// <param name="count"></param>
        private void SleepWhile(int count)
        {
            for (int i = 0; i < count; i++) { }
        }
    }
}
