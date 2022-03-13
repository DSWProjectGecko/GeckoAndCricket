using System;
using System.Collections.Generic;
using BaseScripts;
using UnityEditor;
using System.Reflection;
using System.Runtime.InteropServices;
using PlayerScripts;
using UnityEngine;
using Object = UnityEngine.Object;
using Component = System.ComponentModel.Component;

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
        private readonly BaseCharacter _characterObject;
        
        // Class stuff:
        private int _counter;

        private static List<DebugUtility> _instances;
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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DebugUtility(BaseCharacter character, DebugType type)
        {
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
            
            switch (type)
            {
                case DebugType.Collision:
                    InitializeCollisionDebug();
                    break;
                case DebugType.Movement:
                    break;
                case DebugType.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            _counter = 0;
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
            string message = "";
            int flagIndex = -1;
            foreach (string str in _messageStrings)
            {
                message += str;
                if (flagIndex > -1)
                {
                    unsafe
                    {
                        bool value = *_flagPointers[flagIndex];
                        message += value;
                    }
                }
                
                flagIndex++;
            }
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
            if (!(_characterObject is Player)) 
                return;
            
            _characterObject.health = 9999f;
            ((Player)_characterObject).maxStamina = 9999f;
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
    }
}
#endif
