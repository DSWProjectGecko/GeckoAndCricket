using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BaseScripts;
using PlayerScripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#if DEBUG
namespace DebugUtility
{
    public enum DebugType
    {
        All, Collision, Movement
    }

    public class DebugUtility
    {
        // Object related:
        private string[] _messageStrings;
        private unsafe bool*[] _flagPointers;
        private unsafe float*[] _varPointers;
        private readonly BaseCharacter _characterObject;

        // FPS Stuff
        private Text _fpsCounter;
        private float _deltaTime;

        // Class stuff:
        private int _counter;
        private bool _logFile;
        private readonly StreamWriter _file;

        private static List<DebugUtility> _instances;
        private readonly DebugType _type;
        private static bool _fpsLimit;

        private static MethodInfo _clearConsoleMethod;
        private static MethodInfo ClearConsoleMethod 
        {
            get 
            {
                if (_clearConsoleMethod != null) return _clearConsoleMethod;
                
                Assembly assembly = Assembly.GetAssembly (typeof(SceneView));
                Type logEntries = assembly.GetType ("UnityEditor.LogEntries");
                _clearConsoleMethod = logEntries.GetMethod ("Clear");
                return _clearConsoleMethod;
            }
        }

        /// <summary>
        /// Initializes a debug mode for a specific character object
        /// </summary>
        /// <param name="character">Character object</param>
        /// <param name="type">Debug type</param>
        /// <param name="writeToLogFile">Write to log file</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DebugUtility(BaseCharacter character, DebugType type, bool writeToLogFile)
        {
            _type = type;
            if (!_fpsLimit)
            {
                Application.targetFrameRate = 60;
                _fpsLimit = true;
            }
            
            if (_instances == null)
            {
                _instances = new List<DebugUtility> {this};
            }
            else
            {
                _instances.Add(this);
            }

            _characterObject = character;
            
            switch (_type)
            {
                case DebugType.Collision:
                    InitializeCollisionDebug();
                    break;
                case DebugType.Movement:
                    InitializeMovementDebug();
                    break;
                case DebugType.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            _counter = 0;
            
            if (!writeToLogFile) 
                return;
            
            _logFile = true;
            string fileDir = Directory.GetCurrentDirectory() + @"\DebugLog.txt";

            if (!File.Exists(fileDir))
                File.Open(fileDir, FileMode.Create).Close();
            
            _file = File.AppendText(fileDir);
        }

        ~DebugUtility()
        {
            if (_logFile)
            {
                _file.Close();
            }
        }

        /// <summary>
        /// Clears Unity console.
        /// </summary>
        public static void ClearLogConsole()
        {
            GameObject invoker = new GameObject
            {
                name = "Invoke Clear Console."
            };
            
            ClearConsoleMethod.Invoke (invoker, null);
            Object.DestroyImmediate(invoker, false);
        }

        /// <summary>
        /// Prints a debug message to the Unity console.
        /// </summary>
        /// <param name="clearConsole">if true, console will be cleared</param>
        public void PrintDebug(bool clearConsole)
        {
            if (_counter < 60)
            {
                _counter++;
                return;
            }

            _counter = 0;
            string message = GenerateMessage();
            if (clearConsole)
                ClearLogConsole();
            Debug.Log(message);
        }

        public static void PrintAllDebug(bool clearConsole)
        {
            if (_instances == null)
                return;

            if (clearConsole)
                ClearLogConsole();
            
            foreach (DebugUtility instance in _instances)
            {
                instance.PrintDebug(false);
            }
        }

        public void UseCheats()
        {
            // ReSharper disable once MergeCastWithTypeCheck
            if (!(_characterObject is Player)) 
                return;
            
            _characterObject.health = 9999f;
            ((Player)_characterObject).maxStamina = 9999f;
        }
        
        public void ShowFPS(ref Text fpsText)
        {
            _fpsCounter = fpsText;
            _fpsCounter.text = "";
        }

        public void PrintFPS()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
            float fps = 1.0f / _deltaTime;
            _fpsCounter.text = Mathf.Ceil(fps).ToString(CultureInfo.CurrentCulture);
        }

        public void WriteToFile(int frequency)
        {
            if (!_logFile)
                return;
            
            if (_counter < frequency)
            {
                _counter++;
                return;
            }
            _counter = 0;

            _file.WriteLine(Time.time + " " + GenerateMessage());
        }
        
        public void SaveFile()
        {
            if (!_logFile) 
                return;
            
            _file.Close();
            _logFile = false;
        }

        private string GenerateMessage()
        {
            string message = "";
            int varIndex = -1;
            foreach (string str in _messageStrings)
            {
                message += str;
                if (varIndex > -1)
                {
                    unsafe
                    {
                        string value = _type switch
                        {
                            DebugType.Collision => Convert.ToString(*_flagPointers[varIndex]),
                            DebugType.Movement => Convert.ToString(*_varPointers[varIndex], CultureInfo.CurrentCulture),
                            _ => string.Empty
                        };
                        message += value;
                    }
                }
                
                varIndex++;
            }
            
            return message;
        }
        
        
        private unsafe void InitializeCollisionDebug()
        {
            _flagPointers = new []
            {
                _characterObject.GetIsGroundedPointer(),
                _characterObject.GetIsTouchingWallPointer(),
                _characterObject.GetIsTouchingCeilingPointer(),
                _characterObject.GetIsAttachedToRopePointer()
            };

            _messageStrings = new[]
            {
                _characterObject.name,
                @" | IsGrounded = ",
                @" | IsTouchingWall = ",
                @" | IsTouchingCeiling = ",
                @" | IsAttachedToRope = "
            };
        }
        
        private unsafe void InitializeMovementDebug()
        {
            if (!(_characterObject is Player player)) 
                return;

            _varPointers = new[]
            {
                player.GetMovementSpeedPointer(),
                player.XValue,
                player.CurrentSpeedValue,
                player.AccelerationValue,
                player.SpeedDifferenceValue,
                player.VelocityForceValue

            };

            _messageStrings = new[]
            {
                player.name,
                @" | MovementSpeed = ",
                @" | X = ",
                @" | CurrentSpeed = ",
                @" | Acceleration = ",
                @" | SpeedDifference = ",
                @" | VelocityForce = "
            };
        }
    }
}
#endif
