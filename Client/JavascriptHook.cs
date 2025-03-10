﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.ClearScript.V8;

using GTA;
using GTA.Native;
using GTA.Math;

namespace CoopClient
{
    /// <summary>
    /// Don't use this!
    /// </summary>
    public class JavascriptHook : Script
    {
        private static readonly List<V8ScriptEngine> _scriptEngines = new List<V8ScriptEngine>();
        internal static bool JavascriptLoaded { get; private set; } = false;

        /// <summary>
        /// Don't use this!
        /// </summary>
        public JavascriptHook()
        {
            Tick += Ontick;
        }

        private void Ontick(object sender, EventArgs e)
        {
            if (!Main.MainNetworking.IsOnServer() || _scriptEngines.Count == 0)
            {
                return;
            }

            lock (_scriptEngines)
            {
                _scriptEngines.ForEach(engine =>
                {
                    try
                    {
                        engine.Script.API.InvokeTick();
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                });
            }
        }

        internal static void LoadAll()
        {
            string serverAddress = Main.MainSettings.LastServerAddress.Replace(":", ".");

            if (!Directory.Exists("scripts\\resources\\" + serverAddress))
            {
                try
                {
                    Directory.CreateDirectory("scripts\\resources\\" + serverAddress);
                }
                catch (Exception ex)
                {
                    GTA.UI.Notification.Show("~r~~h~Javascript Error");
                    Logger.Write(ex.Message, Logger.LogLevel.Server);

                    // Without the directory we can't do the other stuff
                    return;
                }
            }

            lock (_scriptEngines)
            {
                foreach (string script in Directory.GetFiles("scripts\\resources\\" + serverAddress, "*.js"))
                {
                    try
                    {
                        V8ScriptEngine engine = new V8ScriptEngine()
                        {
                            AccessContext = typeof(ScriptContext)
                        };

                        engine.AddHostObject("API", new ScriptContext());

                        engine.AddHostType(typeof(Dictionary<,>));

                        // SHVDN
                        engine.AddHostType(typeof(Vector3));
                        engine.AddHostType(typeof(Quaternion));

                        engine.Execute(File.ReadAllText(script));

                        try
                        {
                            engine.Script.API.InvokeStart();
                        }
                        catch (Exception ex)
                        {
                            GTA.UI.Notification.Show("~r~~h~Javascript Error");
                            Logger.Write(ex.Message, Logger.LogLevel.Server);
                        }
                        
                        _scriptEngines.Add(engine);
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                }
            }

            JavascriptLoaded = true;
        }

        internal static void StopAll()
        {
            lock (_scriptEngines)
            {
                _scriptEngines.ForEach(engine =>
                {
                    try
                    {
                        engine.Script.API.InvokeStop();
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                    
                    engine.Dispose();
                });
                _scriptEngines.Clear();
            }

            JavascriptLoaded = false;
        }

        internal static void InvokePlayerConnect(string username, long nethandle)
        {
            lock (_scriptEngines)
            {
                _scriptEngines.ForEach(engine =>
                {
                    try
                    {
                        engine.Script.API.InvokePlayerConnect(username, nethandle);
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                });
            }
        }

        internal static void InvokePlayerDisonnect(string username, long nethandle, string reason = null)
        {
            lock (_scriptEngines)
            {
                _scriptEngines.ForEach(engine =>
                {
                    try
                    {
                        engine.Script.API.InvokePlayerDisonnect(username, nethandle, reason);
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                });
            }
        }

        internal static void InvokeChatMessage(string from, string message)
        {
            lock (_scriptEngines)
            {
                _scriptEngines.ForEach(engine =>
                {
                    try
                    {
                        engine.Script.API.InvokeChatMessage(from, message);
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                });
            }
        }

        internal static void InvokeServerEvent(string eventName, object[] args)
        {
            lock (_scriptEngines)
            {
                _scriptEngines.ForEach(engine =>
                {
                    try
                    {
                        engine.Script.API.InvokeServerEvent(eventName, args);
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show("~r~~h~Javascript Error");
                        Logger.Write(ex.Message, Logger.LogLevel.Server);
                    }
                });
            }
        }
    }

    internal class ScriptContext
    {
        #region DELEGATES
        public delegate void EmptyEvent();
        public delegate void PlayerConnectEvent(string username, long nethandle, string reason);
        public delegate void ChatMessageEvent(string from, string message);
        #endregion

        #region EVENTS
        private Dictionary<string, Action<object[]>> _serverEvents = new Dictionary<string, Action<object[]>>();
        public event EmptyEvent OnStart, OnStop, OnTick;
        public event PlayerConnectEvent OnPlayerConnect, OnPlayerDisconnect;
        public event ChatMessageEvent OnChatMessage;

        internal void InvokeStart()
        {
            OnStart?.Invoke();
        }

        internal void InvokeStop()
        {
            OnStop?.Invoke();
        }

        internal void InvokeTick()
        {
            OnTick?.Invoke();
        }

        internal void InvokePlayerConnect(string username, long nethandle)
        {
            OnPlayerConnect?.Invoke(username, nethandle, null);
        }

        internal void InvokePlayerDisonnect(string username, long nethandle, string reason)
        {
            OnPlayerDisconnect?.Invoke(username, nethandle, reason);
        }

        internal void InvokeChatMessage(string from, string message)
        {
            OnChatMessage?.Invoke(from, message);
        }

        internal void InvokeServerEvent(string eventName, object[] args)
        {
            _serverEvents.FirstOrDefault(x => x.Key == eventName).Value?.Invoke(args);
        }
        #endregion

        /* ===== PLAYER STUFF ===== */
        public void SendLocalMessage(string message)
        {
            Main.MainChat.AddMessage("JAVASCRIPT", message);
        }

        public string GetLocalUsername()
        {
            return Main.MainSettings.Username;
        }

        public long GetLocalNetHandle()
        {
            return Main.LocalNetHandle;
        }

        // This only applies to server-side created objects
        public void CleanUpWorld()
        {
            Main.CleanUpWorld();
        }

        // This create an object to delete it with CleanUpWorld() or on disconnect
        public void CreateObject(string hash, params object[] args)
        {
            if (!Hash.TryParse(hash, out Hash ourHash) || !Main.CheckNativeHash.ContainsKey((ulong)ourHash))
            {
                GTA.UI.Notification.Show("~r~~h~Javascript Error");
                Logger.Write($"Hash \"{ourHash}\" has not been found!", Logger.LogLevel.Server);
                return;
            }

            int result = Function.Call<int>(ourHash, args.Select(o => new InputArgument(o)).ToArray());

            foreach (KeyValuePair<ulong, byte> checkHash in Main.CheckNativeHash)
            {
                if (checkHash.Key == (ulong)ourHash)
                {
                    lock (Main.ServerItems)
                    {
                        Main.ServerItems.Add(result, checkHash.Value);
                    }
                    break;
                }
            }
        }

        public void SendNotification(string message)
        {
            GTA.UI.Notification.Show(message);
        }
        public void SendNotification(string[] messages)
        {
            SendNotification(string.Concat(messages));
        }

        public bool IsPlayerInvincible()
        {
            return Game.Player.Character?.IsInvincible ?? false;
        }
        public void SetPlayerInvincible(bool invincible)
        {
            Game.Player.Character.IsInvincible = invincible;
        }

        public Vector3? GetPlayerPosition()
        {
            return Game.Player.Character?.Position;
        }
        public Vector3? GetPlayerRotation()
        {
            return Game.Player.Character?.Rotation;
        }

        public void SetPlayerPosition(Vector3 position)
        {
            Game.Player.Character.Position = position;
        }
        public void SetPlayerPositionNoOffset(Vector3 position)
        {
            Game.Player.Character.PositionNoOffset = position;
        }
        public void SetPlayerRotation(Vector3 rotation)
        {
            Game.Player.Character.Rotation = rotation;
        }

        public bool IsPlayerInAnyVehicle()
        {
            return Game.Player.Character.IsInVehicle();
        }

        public int? GetCharachterHandle()
        {
            return Game.Player.Character?.Handle;
        }

        public int? CreateVehicle(int hash, Vector3 position, Quaternion? Rotation = null)
        {
            Model model = hash.ModelRequest();
            if (model == null)
            {
                return null;
            }

            Vehicle veh = World.CreateVehicle(model, position);
            model.MarkAsNoLongerNeeded();
            if (veh == null)
            {
                return null;
            }

            if (Rotation != null)
            {
                veh.Quaternion = Rotation.Value;
            }

            return veh.Handle;
        }

        public Vector3? GetVehiclePosition()
        {
            return Game.Player.Character.CurrentVehicle?.Position;
        }
        public Vector3? GetVehicleRotation()
        {
            return Game.Player.Character.CurrentVehicle?.Rotation;
        }

        public void SetVehiclePosition(Vector3 position)
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.Position = position;
            }
        }
        public void SetVehiclePositionNoOffset(Vector3 position)
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.PositionNoOffset = position;
            }
        }
        public void SetVehicleRotation(Vector3 rotation)
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.Rotation = rotation;
            }
        }

        public int? GetVehicleHandle()
        {
            return Game.Player.Character?.CurrentVehicle?.Handle;
        }

        public int? GetVehicleSeatIndex()
        {
            return (int)Game.Player.Character?.SeatIndex;
        }

        public void SetVehicleEngineStatus(bool turnedOn)
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.IsEngineRunning = turnedOn;
            }
        }
        public bool GetVehicleEngineStatus()
        {
            return Game.Player.Character.CurrentVehicle?.IsEngineRunning ?? false;
        }

        public float? GetVehicleHeightAboveGround()
        {
            return Game.Player.Character.CurrentVehicle?.HeightAboveGround;
        }

        public string GetVehicleType()
        {
            Vehicle veh = Game.Player.Character?.CurrentVehicle;
            if (veh == null)
            {
                return null;
            }

            return Enum.GetName(typeof(VehicleType), veh.Type);
        }

        public void RepairVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                Game.Player.Character.CurrentVehicle.Repair();
            }
        }

        public void GivePlayerWeapon(uint hash, int ammoCount, bool equipNow, bool isAmmoLoaded)
        {
            Game.Player.Character.Weapons.Give((WeaponHash)hash, ammoCount, equipNow, isAmmoLoaded);
        }

        public void SetPlayerHealth(int health)
        {
            Game.Player.Character.Health = health;
        }
        public void SetPlayerHealth(float health)
        {
            Game.Player.Character.HealthFloat = health;
        }

        public void SetPlayerArmor(int armor)
        {
            Game.Player.Character.Armor = armor;
        }
        public void SetPlayerArmor(float armor)
        {
            Game.Player.Character.ArmorFloat = armor;
        }

        /* ===== OTHER PLAYER STUFF ===== */
        public Vector3 GetPlayerPosition(long nethandle)
        {
            lock (Main.Players)
            {
                return Main.Players.ContainsKey(nethandle) ? Main.Players.First(x => x.Key == nethandle).Value.Position : default;
            }
        }
        public Vector3 GetPlayerRotation(long nethandle)
        {
            lock (Main.Players)
            {
                return Main.Players.ContainsKey(nethandle) ? Main.Players.First(x => x.Key == nethandle).Value.Rotation : default;
            }
        }

        // Get nethandle and charachter handle
        public Dictionary<long, int> GetAllNearbyPlayers(float distance)
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            
            lock (Main.Players)
            {
                Vector3 localPosition = Game.Player.Character.Position;
                foreach (KeyValuePair<long, Entities.Player.EntitiesPlayer> player in Main.Players)
                {
                    if (player.Value.Position.DistanceTo2D(localPosition) < distance)
                    {
                        // Character handle = 0 (if no character exists)
                        result.Add(player.Key, player.Value.Character?.Handle ?? 0);
                    }
                }
            }

            return result;
        }

        public long GetNetHandleByUsername(string username)
        {
            lock (Main.Players)
            {
                return Main.Players.FirstOrDefault(x => x.Value.Username == username).Key;
            }
        }

        /* ===== OTHER STUFF ===== */
        public bool IsControlJustReleased(int control)
        {
            return Game.IsControlJustReleased((Control)control);
        }

        public bool IsControlJustPressed(int control)
        {
            return Game.IsControlJustPressed((Control)control);
        }

        public bool IsControlPressed(int control)
        {
            return Game.IsControlPressed((Control)control);
        }

        public bool AnyMapLoaded()
        {
            return MapLoader.AnyMapLoaded();
        }

        public void LoadMap(string name)
        {
            MapLoader.LoadMap(name);
        }

        public void UnloadMap()
        {
            MapLoader.UnloadMap();
        }

        private void AddServerEvent(string eventName, Action<object[]> action)
        {
            if (!_serverEvents.ContainsKey(eventName))
            {
                _serverEvents.Add(eventName, action);
            }
        }
        public void AddServerEvent(string eventName, dynamic action)
        {
            AddServerEvent(eventName, arg => action(arg));
        }

        public void SendTriggerEvent(string eventName, params object[] args)
        {
            Main.MainNetworking.SendTriggerEvent(eventName, args);
        }
    }
}
