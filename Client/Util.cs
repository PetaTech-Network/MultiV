﻿using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using GTA;
using GTA.Native;
using GTA.Math;

namespace CoopClient
{
    enum ETasks
    {
        CLIMB_LADDER = 47
    }

    static class Util
    {
        #region -- POINTER --
        private static int _steeringAngleOffset { get; set; }

        public static unsafe void NativeMemory()
        {
            IntPtr address;

            address = Game.FindPattern("\x74\x0A\xF3\x0F\x11\xB3\x1C\x09\x00\x00\xEB\x25", "xxxxxx????xx");
            if (address != IntPtr.Zero)
            {
                _steeringAngleOffset = *(int*)(address + 6) + 8;
            }

            address = Game.FindPattern("\x32\xc0\xf3\x0f\x11\x09", "xxxxxx"); // Weapon / Radio slowdown
            if (address != IntPtr.Zero)
            {
                for (int i = 0; i < 6; i++)
                {
                    *(byte*)(address + i).ToPointer() = 0x90;
                }
            }
        }

        public static unsafe void CustomSteeringAngle(this Vehicle veh, float value)
        {
            IntPtr address = new IntPtr((long)veh.MemoryAddress);
            if (address == IntPtr.Zero || _steeringAngleOffset == 0)
            {
                return;
            }

            *(float*)(address + _steeringAngleOffset).ToPointer() = value;
        }
        #endregion

        public static (byte, byte[]) GetBytesFromObject(object obj)
        {
            switch (obj)
            {
                case byte _:
                    return (0x01, BitConverter.GetBytes((byte)obj));
                case short _:
                    return (0x02, BitConverter.GetBytes((short)obj));
                case ushort _:
                    return (0x03, BitConverter.GetBytes((ushort)obj));
                case int _:
                    return (0x04, BitConverter.GetBytes((int)obj));
                case uint _:
                    return (0x05, BitConverter.GetBytes((uint)obj));
                case long _:
                    return (0x06, BitConverter.GetBytes((long)obj));
                case ulong _:
                    return (0x07, BitConverter.GetBytes((ulong)obj));
                case float _:
                    return (0x08, BitConverter.GetBytes((float)obj));
                case bool _:
                    return (0x09, BitConverter.GetBytes((bool)obj));
                default:
                    return (0x0, null);
            }
        }

        public static string[] GetReloadingAnimation(this Ped ped)
        {
            switch (ped.Weapons.Current.Hash)
            {
                case WeaponHash.Revolver:
                case WeaponHash.RevolverMk2:
                case WeaponHash.DoubleActionRevolver:
                case WeaponHash.NavyRevolver:
                    return new string[2] { "anim@weapons@pistol@revolver_str", "reload_aim" };
                case WeaponHash.APPistol:
                    return new string[2] { "weapons@pistol@ap_pistol_str", "reload_aim" };
                case WeaponHash.Pistol50:
                    return new string[2] { "weapons@pistol@pistol_50_str", "reload_aim" };
                case WeaponHash.Pistol:
                case WeaponHash.PistolMk2:
                case WeaponHash.PericoPistol:
                case WeaponHash.SNSPistol:
                case WeaponHash.SNSPistolMk2:
                case WeaponHash.HeavyPistol:
                case WeaponHash.VintagePistol:
                case WeaponHash.CeramicPistol:
                case WeaponHash.MachinePistol:
                    return new string[2] { "weapons@pistol@pistol_str", "reload_aim" };
                case WeaponHash.AssaultRifle:
                case WeaponHash.AssaultrifleMk2:
                    return new string[2] { "weapons@rifle@hi@assault_rifle_str", "reload_aim" };
                case WeaponHash.SniperRifle:
                    return new string[2] { "weapons@rifle@hi@sniper_rifle_str", "reload_aim" };
                case WeaponHash.HeavySniper:
                case WeaponHash.HeavySniperMk2:
                    return new string[2] { "weapons@rifle@lo@sniper_heavy_str", "reload_aim" };
                case WeaponHash.PumpShotgun:
                case WeaponHash.PumpShotgunMk2:
                    return new string[2] { "weapons@rifle@pump_str", "reload_aim" };
                case WeaponHash.Railgun:
                    return new string[2] { "weapons@rifle@lo@rail_gun_str", "reload_aim" };
                case WeaponHash.SawnOffShotgun:
                    return new string[2] { "weapons@rifle@lo@sawnoff_str", "reload_aim" };
                case WeaponHash.AssaultShotgun:
                    return new string[2] { "weapons@rifle@lo@shotgun_assault_str", "reload_aim" };
                case WeaponHash.BullpupShotgun:
                    return new string[2] { "weapons@rifle@lo@shotgun_bullpup_str", "reload_aim" };
                case WeaponHash.AdvancedRifle:
                    return new string[2] { "weapons@submg@advanced_rifle_str", "reload_aim" };
                case WeaponHash.CarbineRifle:
                case WeaponHash.CarbineRifleMk2:
                case WeaponHash.CompactRifle:
                    return new string[2] { "weapons@rifle@lo@carbine_str", "reload_aim" };
                case WeaponHash.Gusenberg:
                    return new string[2] { "anim@weapons@machinegun@gusenberg_str", "reload_aim" };
                case WeaponHash.Musket:
                    return new string[2] { "anim@weapons@musket@musket_str", "reload_aim" };
                case WeaponHash.FlareGun:
                    return new string[2] { "anim@weapons@pistol@flare_str", "reload_aim" };
                case WeaponHash.SpecialCarbine:
                case WeaponHash.SpecialCarbineMk2:
                    return new string[2] { "anim@weapons@rifle@lo@spcarbine_str", "reload_aim" };
                case WeaponHash.CombatPDW:
                    return new string[2] { "anim@weapons@rifle@lo@pdw_str", "reload_aim" };
                case WeaponHash.BullpupRifle:
                case WeaponHash.BullpupRifleMk2:
                    return new string[2] { "anim@weapons@submg@bullpup_rifle_str", "reload_aim" };
                case WeaponHash.AssaultSMG:
                    return new string[2] { "weapons@submg@assault_smg_str", "reload_aim" };
                case WeaponHash.MicroSMG:
                case WeaponHash.MiniSMG:
                    return new string[2] { "weapons@submg@lo@micro_smg_str", "reload_aim" };
                case WeaponHash.SMG:
                case WeaponHash.SMGMk2:
                    return new string[2] { "weapons@rifle@smg_str", "reload_aim" };
                case WeaponHash.GrenadeLauncher:
                case WeaponHash.GrenadeLauncherSmoke:
                case WeaponHash.CompactGrenadeLauncher:
                    return new string[2] { "weapons@heavy@grenade_launcher_str", "reload_aim" };
                case WeaponHash.RPG:
                case WeaponHash.Firework:
                    return new string[2] { "weapons@heavy@rpg_str", "reload_aim" };
                case WeaponHash.CombatMG:
                case WeaponHash.CombatMGMk2:
                    return new string[2] { "weapons@machinegun@combat_mg_str", "reload_aim" };
                case WeaponHash.MG:
                    return new string[2] { "weapons@machinegun@mg_str", "reload_aim" };
                default:
                    GTA.UI.Notification.Show($"~r~Reloading failed! Weapon ~g~[{ped.Weapons.Current.Hash}]~r~ could not be found!");
                    return null;
            }
        }

        public static string LoadAnim(this string anim)
        {
            ulong startTime = GetTickCount64();
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, anim))
            {
                Script.Yield();
                Function.Call(Hash.REQUEST_ANIM_DICT, anim);
                if (GetTickCount64() - startTime >= 1000)
                {
                    break;
                }
            }

            return anim;
        }

        public static Model ModelRequest(this int hash)
        {
            Model model = new Model(hash);

            if (!model.IsValid)
            {
                //GTA.UI.Notification.Show("~y~Not valid!");
                return null;
            }

            if (!model.IsLoaded)
            {
                return model.Request(1000) ? model : null;
            }

            return model;
        }

        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0 && Comparer<T>.Default.Compare(item, end) <= 0;
        }

        public static bool Compare<T, Y>(this Dictionary<T, Y> item, Dictionary<T, Y> item2)
        {
            if (item == null || item2 == null || item.Count != item2.Count)
            {
                return false;
            }

            foreach (KeyValuePair<T, Y> pair in item)
            {
                if (item2.TryGetValue(pair.Key, out Y value) && Equals(value, pair.Value))
                {
                    continue;
                }

                // TryGetValue() or Equals failed
                return false;
            }

            // No difference between item and item2
            return true;
        }

        public static Vector3 LinearVectorLerp(Vector3 start, Vector3 end, ulong currentTime, int duration)
        {
            return new Vector3()
            {
                X = LinearFloatLerp(start.X, end.X, currentTime, duration),
                Y = LinearFloatLerp(start.Y, end.Y, currentTime, duration),
                Z = LinearFloatLerp(start.Z, end.Z, currentTime, duration),
            };
        }

        public static float LinearFloatLerp(float start, float end, ulong currentTime, int duration)
        {
            return (end - start) * currentTime / duration + start;
        }

        public static float Lerp(float from, float to, float fAlpha)
        {
            return (from * (1.0f - fAlpha)) + (to * fAlpha); //from + (to - from) * fAlpha
        }

        public static int GetResponsiblePedHandle(this Vehicle veh)
        {
            if (veh == null || veh.Handle == 0 || !veh.Exists())
            {
                return 0;
            }

            if (!veh.IsSeatFree(VehicleSeat.Driver))
            {
                return veh.GetPedOnSeat(VehicleSeat.Driver).Handle;
            }

            for (int i = 0; i < veh.PassengerCapacity; i++)
            {
                if (!veh.IsSeatFree((VehicleSeat)i))
                {
                    return veh.GetPedOnSeat((VehicleSeat)i).Handle;
                }
            }

            return 0;
        }

        public static byte GetPedSpeed(this Ped ped)
        {
            if (ped.IsSprinting)
            {
                return 3;
            }
            if (ped.IsRunning)
            {
                return 2;
            }
            if (ped.IsWalking)
            {
                return 1;
            }
            
            return 0;
        }

        public static Vector3 GetPedAimCoords(this Ped ped, bool isNpc)
        {
            bool aimOrShoot = ped.IsAiming || ped.IsShooting && ped.Weapons.Current?.AmmoInClip != 0;
            return aimOrShoot ? (isNpc ? GetLastWeaponImpact(ped) : RaycastEverything(new Vector2(0, 0))) : new Vector3();
        }

        /// <summary>
        /// Only works for players NOT NPCs
        /// </summary>
        /// <returns>Vector3</returns>
        public static Vector3 GetVehicleAimCoords()
        {
            return RaycastEverything(new Vector2(0, 0));
        }

        public static ushort? GetVehicleFlags(this Ped ped, Vehicle veh)
        {
            ushort? flags = 0;

            if (veh.IsEngineRunning)
            {
                flags |= (ushort)VehicleDataFlags.IsEngineRunning;
            }

            if (veh.AreLightsOn)
            {
                flags |= (ushort)VehicleDataFlags.AreLightsOn;
            }

            if (veh.BrakePower >= 0.01f)
            {
                flags |= (ushort)VehicleDataFlags.AreBrakeLightsOn;
            }

            if (veh.AreHighBeamsOn)
            {
                flags |= (ushort)VehicleDataFlags.AreHighBeamsOn;
            }

            if (veh.IsSirenActive)
            {
                flags |= (ushort)VehicleDataFlags.IsSirenActive;
            }

            if (veh.IsDead)
            {
                flags |= (ushort)VehicleDataFlags.IsDead;
            }

            if (Function.Call<bool>(Hash.IS_HORN_ACTIVE, veh.Handle))
            {
                flags |= (ushort)VehicleDataFlags.IsHornActive;
            }

            if (veh.IsSubmarineCar && Function.Call<bool>(Hash._GET_IS_SUBMARINE_VEHICLE_TRANSFORMED, veh.Handle))
            {
                flags |= (ushort)VehicleDataFlags.IsTransformed;
            }

            if (veh.HasRoof && (veh.RoofState == VehicleRoofState.Opened || veh.RoofState == VehicleRoofState.Opening))
            {
                flags |= (ushort)VehicleDataFlags.RoofOpened;
            }

            if (veh.IsTurretSeat((int)ped.SeatIndex))
            {
                flags |= (ushort)VehicleDataFlags.OnTurretSeat;
            }

            if (veh.IsPlane)
            {
                flags |= (ushort)VehicleDataFlags.IsPlane;
            }

            return flags;
        }

        public static ushort? GetPedFlags(this Ped ped, bool isPlayer = false)
        {
            ushort? flags = 0;

            if (ped.IsAiming)
            {
                flags |= (ushort)PedDataFlags.IsAiming;
            }

            if ((ped.IsShooting || isPlayer && Game.IsControlPressed(Control.Attack)) && ped.Weapons.Current?.AmmoInClip != 0)
            {
                flags |= (ushort)PedDataFlags.IsShooting;
            }

            if (ped.IsReloading)
            {
                flags |= (ushort)PedDataFlags.IsReloading;
            }

            if (ped.IsJumping)
            {
                flags |= (ushort)PedDataFlags.IsJumping;
            }

            if (ped.IsRagdoll)
            {
                flags |= (ushort)PedDataFlags.IsRagdoll;
            }

            if (ped.IsOnFire)
            {
                flags |= (ushort)PedDataFlags.IsOnFire;
            }

            if (ped.IsInParachuteFreeFall)
            {
                flags |= (ushort)PedDataFlags.IsInParachuteFreeFall;
            }

            if (ped.ParachuteState == ParachuteState.Gliding)
            {
                flags |= (ushort)PedDataFlags.IsParachuteOpen;
            }

            if (Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped.Handle, ETasks.CLIMB_LADDER)) // USING_LADDER
            {
                flags |= (ushort)PedDataFlags.IsOnLadder;
            }

            if (ped.IsVaulting && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped.Handle, ETasks.CLIMB_LADDER))
            {
                flags |= (ushort)PedDataFlags.IsVaulting;
            }

            return flags;
        }

        public static Dictionary<byte, short> GetPedClothes(this Ped ped)
        {
            Dictionary<byte, short> result = new Dictionary<byte, short>();
            for (byte i = 0; i < 11; i++)
            {
                short mod = Function.Call<short>(Hash.GET_PED_DRAWABLE_VARIATION, ped.Handle, i);
                result.Add(i, mod);
            }
            return result;
        }

        public static Dictionary<uint, bool> GetWeaponComponents(this Weapon weapon)
        {
            Dictionary<uint, bool> result = null;

            if (weapon.Components.Count > 0)
            {
                result = new Dictionary<uint, bool>();

                foreach (var comp in weapon.Components)
                {
                    result.Add((uint)comp.ComponentHash, comp.Active);
                }
            }

            return result;
        }

        public static Dictionary<int, int> GetVehicleMods(this VehicleModCollection mods)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            foreach (VehicleMod mod in mods.ToArray())
            {
                result.Add((int)mod.Type, mod.Index);
            }
            return result;
        }

        public static VehicleDamageModel GetVehicleDamageModel(this Vehicle veh)
        {
            // Broken windows
            byte brokenWindows = 0;
            for (int i = 0; i < 8; i++)
            {
                if (!veh.Windows[(VehicleWindowIndex)i].IsIntact)
                {
                    brokenWindows |= (byte)(1 << i);
                }
            }

            // Broken doors
            byte brokenDoors = 0;
            foreach (VehicleDoor door in veh.Doors)
            {
                if (door.IsBroken)
                {
                    brokenDoors |= (byte)(1 << (byte)door.Index);
                }
            }

            // Bursted tires
            short burstedTires = 0;
            foreach (VehicleWheel wheel in veh.Wheels.GetAllWheels())
            {
                if (wheel.IsBursted)
                {
                    burstedTires |= (short)(1 << (int)wheel.BoneId);
                }
            }

            return new VehicleDamageModel()
            {
                BrokenDoors = brokenDoors,
                BrokenWindows = brokenWindows,
                BurstedTires = burstedTires,
                LeftHeadLightBroken = (byte)(veh.IsLeftHeadLightBroken ? 1 : 0),
                RightHeadLightBroken = (byte)(veh.IsRightHeadLightBroken ? 1 : 0)
            };
        }

        public static void SetVehicleDamageModel(this Vehicle veh, VehicleDamageModel model, bool leavedoors = true)
        {
            for (int i = 0; i < 8; i++)
            {
                if ((model.BrokenDoors & (byte)(1 << i)) != 0)
                {
                    veh.Doors[(VehicleDoorIndex)i].Break(leavedoors);
                }
                else if (veh.Doors[(VehicleDoorIndex)i].IsBroken)
                {
                    // The vehicle can only fix a door if the vehicle was completely fixed
                    veh.Repair();
                    return;
                }

                if ((model.BrokenWindows & (byte)(1 << i)) != 0)
                {
                    veh.Windows[(VehicleWindowIndex)i].Smash();
                }
                else if (!veh.Windows[(VehicleWindowIndex)i].IsIntact)
                {
                    veh.Windows[(VehicleWindowIndex)i].Repair();
                }
            }

            foreach (VehicleWheel wheel in veh.Wheels)
            {
                if ((model.BurstedTires & (short)(1 << (int)wheel.BoneId)) != 0)
                {
                    if (!wheel.IsBursted)
                    {
                        wheel.Puncture();
                        wheel.Burst();
                    }
                }
                else if (wheel.IsBursted)
                {
                    wheel.Fix();
                }
            }

            veh.IsLeftHeadLightBroken = model.LeftHeadLightBroken > 0;
            veh.IsRightHeadLightBroken = model.RightHeadLightBroken > 0;
        }

        public static Settings ReadSettings()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Settings));

            string path = Directory.GetCurrentDirectory() + "\\scripts\\CoopSettings.xml";
            Settings settings = null;

            if (File.Exists(path))
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    settings = (Settings)ser.Deserialize(stream);
                }

                using (FileStream stream = new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite))
                {
                    ser.Serialize(stream, settings);
                }
            }
            else
            {
                using (FileStream stream = File.OpenWrite(path))
                {
                    ser.Serialize(stream, settings = new Settings());
                }
            }

            return settings;
        }

        public static void SaveSettings()
        {
            try
            {
                string path = Directory.GetCurrentDirectory() + "\\scripts\\CoopSettings.xml";

                using (FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Truncate : FileMode.Create, FileAccess.ReadWrite))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Settings));
                    ser.Serialize(stream, Main.MainSettings);
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show("Error saving player settings: " + ex.Message);
            }
        }

        public static Vector3 GetLastWeaponImpact(Ped ped)
        {
            OutputArgument coord = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_PED_LAST_WEAPON_IMPACT_COORD, ped.Handle, coord))
            {
                return new Vector3();
            }

            return coord.GetResult<Vector3>();
        }

        public static bool IsTurretSeat(this Vehicle veh, int seat)
        {
            if (!Function.Call<bool>(Hash.DOES_VEHICLE_HAVE_WEAPONS, veh.Handle))
            {
                return false;
            }

            switch (seat)
            {
                case -1:
                    return (VehicleHash)veh.Model.Hash == VehicleHash.Rhino
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Khanjari
                        || (VehicleHash)veh.Model.Hash == VehicleHash.FireTruck
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Riot2
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Cerberus
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Cerberus2
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Cerberus3;
                case 0:
                    return (VehicleHash)veh.Model.Hash == VehicleHash.Apc;
                case 1:
                    return (VehicleHash)veh.Model.Hash == VehicleHash.Valkyrie
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Valkyrie2
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Technical
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Technical2
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Technical3
                        || (VehicleHash)veh.Model.Hash == VehicleHash.HalfTrack
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Barrage;
                case 2:
                    return (VehicleHash)veh.Model.Hash == VehicleHash.Valkyrie
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Valkyrie2
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Barrage;
                case 3:
                    return (VehicleHash)veh.Model.Hash == VehicleHash.Limo2
                        || (VehicleHash)veh.Model.Hash == VehicleHash.Dinghy5;
                case 7:
                    return (VehicleHash)veh.Model.Hash == VehicleHash.Insurgent;
            }

            return false;
        }

        public static double DegToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            double z = DegToRad(rotation.Z);
            double x = DegToRad(rotation.X);
            double num = Math.Abs(Math.Cos(x));

            return new Vector3
            {
                X = (float)(-Math.Sin(z) * num),
                Y = (float)(Math.Cos(z) * num),
                Z = (float)Math.Sin(x)
            };
        }

        public static bool WorldToScreenRel(Vector3 worldCoords, out Vector2 screenCoords)
        {
            OutputArgument num1 = new OutputArgument();
            OutputArgument num2 = new OutputArgument();

            if (!Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, worldCoords.X, worldCoords.Y, worldCoords.Z, num1, num2))
            {
                screenCoords = new Vector2();
                return false;
            }

            screenCoords = new Vector2((num1.GetResult<float>() - 0.5f) * 2, (num2.GetResult<float>() - 0.5f) * 2);
            return true;
        }

        public static Vector3 ScreenRelToWorld(Vector3 camPos, Vector3 camRot, Vector2 coord)
        {
            Vector3 camForward = RotationToDirection(camRot);
            Vector3 rotUp = camRot + new Vector3(10, 0, 0);
            Vector3 rotDown = camRot + new Vector3(-10, 0, 0);
            Vector3 rotLeft = camRot + new Vector3(0, 0, -10);
            Vector3 rotRight = camRot + new Vector3(0, 0, 10);

            Vector3 camRight = RotationToDirection(rotRight) - RotationToDirection(rotLeft);
            Vector3 camUp = RotationToDirection(rotUp) - RotationToDirection(rotDown);

            double rollRad = -DegToRad(camRot.Y);

            Vector3 camRightRoll = camRight * (float)Math.Cos(rollRad) - camUp * (float)Math.Sin(rollRad);
            Vector3 camUpRoll = camRight * (float)Math.Sin(rollRad) + camUp * (float)Math.Cos(rollRad);

            Vector3 point3D = camPos + camForward * 10.0f + camRightRoll + camUpRoll;
            if (!WorldToScreenRel(point3D, out Vector2 point2D))
            {
                return camPos + camForward * 10.0f;
            }

            Vector3 point3DZero = camPos + camForward * 10.0f;
            if (!WorldToScreenRel(point3DZero, out Vector2 point2DZero))
            {
                return camPos + camForward * 10.0f;
            }

            const double eps = 0.001;
            if (Math.Abs(point2D.X - point2DZero.X) < eps || Math.Abs(point2D.Y - point2DZero.Y) < eps)
            {
                return camPos + camForward * 10.0f;
            }

            float scaleX = (coord.X - point2DZero.X) / (point2D.X - point2DZero.X);
            float scaleY = (coord.Y - point2DZero.Y) / (point2D.Y - point2DZero.Y);

            return camPos + camForward * 10.0f + camRightRoll * scaleX + camUpRoll * scaleY;
        }

        public static Vector3 RaycastEverything(Vector2 screenCoord)
        {
            Vector3 camPos = GameplayCamera.Position;
            Vector3 camRot = GameplayCamera.Rotation;
            const float raycastToDist = 100.0f;
            const float raycastFromDist = 1f;

            Vector3 target3D = ScreenRelToWorld(camPos, camRot, screenCoord);
            Vector3 source3D = camPos;

            Entity ignoreEntity = Game.Player.Character;
            if (Game.Player.Character.IsInVehicle())
            {
                ignoreEntity = Game.Player.Character.CurrentVehicle;
            }

            Vector3 dir = target3D - source3D;
            dir.Normalize();
            RaycastResult raycastResults = World.Raycast(source3D + dir * raycastFromDist,
                source3D + dir * raycastToDist,
                IntersectFlags.Everything,
                ignoreEntity);

            if (raycastResults.DidHit)
            {
                return raycastResults.HitPosition;
            }

            return camPos + dir * raycastToDist;
        }

        [DllImport("kernel32.dll")]
        public static extern ulong GetTickCount64();
    }

    /// <summary>
    /// 
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static Vector3 ToVector(this Quaternion vec)
        {
            return new Vector3()
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public static Quaternion ToQuaternion(this Vector3 vec, float vW = 0.0f)
        {
            return new Quaternion()
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z,
                W = vW
            };
        }

        internal static float Denormalize(this float h)
        {
            return h < 0f ? h + 360f : h;
        }

        internal static float ToRadians(this float val)
        {
            return (float)(Math.PI / 180) * val;
        }

        internal static Vector3 ToRadians(this Vector3 i)
        {
            return new Vector3()
            {
                X = ToRadians(i.X),
                Y = ToRadians(i.Y),
                Z = ToRadians(i.Z),
            };
        }

        internal static Quaternion ToQuaternion(this Vector3 vect)
        {
            vect = new Vector3()
            {
                X = vect.X.Denormalize() * -1,
                Y = vect.Y.Denormalize() - 180f,
                Z = vect.Z.Denormalize() - 180f,
            };

            vect = vect.ToRadians();

            float rollOver2 = vect.Z * 0.5f;
            float sinRollOver2 = (float)Math.Sin((double)rollOver2);
            float cosRollOver2 = (float)Math.Cos((double)rollOver2);
            float pitchOver2 = vect.Y * 0.5f;
            float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
            float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
            float yawOver2 = vect.X * 0.5f; // pitch
            float sinYawOver2 = (float)Math.Sin((double)yawOver2);
            float cosYawOver2 = (float)Math.Cos((double)yawOver2);
            Quaternion result = new Quaternion()
            {
                X = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2,
                Y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2,
                Z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2,
                W = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2
            };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public static LVector3 ToLVector(this Vector3 vec)
        {
            return new LVector3()
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public static LQuaternion ToLQuaternion(this Quaternion vec)
        {
            return new LQuaternion()
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z,
                W = vec.W
            };
        }
    }
}
