﻿using System.Collections.Generic;

using GTA;
using GTA.Native;
using GTA.Math;

namespace CoopClient.Entities.NPC
{
    internal partial class EntitiesNPC
    {
        #region -- VARIABLES --
        /// <summary>
        /// The latest character rotation (may not have been applied yet)
        /// </summary>
        public Vector3 Rotation { get; internal set; }
        internal byte Speed { get; set; }
        private bool _lastIsJumping = false;
        internal bool IsJumping { get; set; }
        internal bool IsRagdoll { get; set; }
        internal bool IsOnFire { get; set; }
        internal bool IsAiming { get; set; }
        internal bool IsShooting { get; set; }
        internal bool IsReloading { get; set; }
        internal uint CurrentWeaponHash { get; set; }
        private Dictionary<uint, bool> _lastWeaponComponents = null;
        internal Dictionary<uint, bool> WeaponComponents { get; set; } = null;
        private int _lastWeaponObj = 0;
        #endregion

        private void DisplayOnFoot()
        {
            if (Character.IsInVehicle())
            {
                Character.Task.LeaveVehicle();
            }

            if (MainVehicle != null)
            {
                MainVehicle = null;
            }

            if (IsOnFire && !Character.IsOnFire)
            {
                Character.IsInvincible = false;

                Function.Call(Hash.START_ENTITY_FIRE, Character.Handle);
            }
            else if (!IsOnFire && Character.IsOnFire)
            {
                Function.Call(Hash.STOP_ENTITY_FIRE, Character.Handle);

                Character.IsInvincible = true;

                if (Character.IsDead)
                {
                    Character.Resurrect();
                }
            }

            if (IsJumping && !_lastIsJumping)
            {
                Character.Task.Jump();
            }

            _lastIsJumping = IsJumping;

            if (IsRagdoll)
            {
                if (!Character.IsRagdoll)
                {
                    // CanRagdoll = true, inside this function
                    Character.Ragdoll();
                }

                return;
            }
            else if (!IsRagdoll && Character.IsRagdoll)
            {
                Character.CanRagdoll = false;
                Character.Task.ClearAllImmediately();

                return;
            }

            if (IsJumping || IsOnFire)
            {
                return;
            }

            if (IsReloading && Character.IsInRange(Position, 0.5f))
            {
                if (!Character.IsReloading)
                {
                    Character.Task.ClearAll();
                    Character.Task.ReloadWeapon();
                }

                return;
            }

            if (Character.Weapons.Current.Hash != (WeaponHash)CurrentWeaponHash || !WeaponComponents.Compare(_lastWeaponComponents))
            {
                Character.Weapons.RemoveAll();

                if (CurrentWeaponHash != (uint)WeaponHash.Unarmed)
                {
                    if (WeaponComponents == null || WeaponComponents.Count == 0)
                    {
                        Character.Weapons.Give((WeaponHash)CurrentWeaponHash, -1, true, true);
                    }
                    else
                    {
                        _lastWeaponObj = Function.Call<int>(Hash.CREATE_WEAPON_OBJECT, CurrentWeaponHash, -1, Position.X, Position.Y, Position.Z, true, 0, 0);

                        foreach (KeyValuePair<uint, bool> comp in WeaponComponents)
                        {
                            if (comp.Value)
                            {
                                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_WEAPON_OBJECT, _lastWeaponObj, comp.Key);
                            }
                        }

                        Function.Call(Hash.GIVE_WEAPON_OBJECT_TO_PED, _lastWeaponObj, Character.Handle);
                    }
                }

                _lastWeaponComponents = WeaponComponents;
            }

            if (IsShooting)
            {
                if (!Character.IsInRange(Position, 0.5f))
                {
                    Function.Call(Hash.TASK_GO_TO_COORD_WHILE_AIMING_AT_COORD, Character.Handle, Position.X, Position.Y,
                                    Position.Z, AimCoords.X, AimCoords.Y, AimCoords.Z, 3f, true, 0x3F000000, 0x40800000, false, 0, false,
                                    unchecked((int)FiringPattern.FullAuto));
                }
                else
                {
                    Function.Call(Hash.TASK_SHOOT_AT_COORD, Character.Handle, AimCoords.X, AimCoords.Y, AimCoords.Z, 1500, unchecked((int)FiringPattern.FullAuto));
                }
            }
            else if (IsAiming)
            {
                if (!Character.IsInRange(Position, 0.5f))
                {
                    Function.Call(Hash.TASK_GO_TO_COORD_WHILE_AIMING_AT_COORD, Character.Handle, Position.X, Position.Y,
                                    Position.Z, AimCoords.X, AimCoords.Y, AimCoords.Z, 3f, false, 0x3F000000, 0x40800000, false, 512, false,
                                    unchecked((int)FiringPattern.FullAuto));
                }
                else
                {
                    Character.Task.AimAt(AimCoords, 100);
                }
            }
            else
            {
                WalkTo();
            }
        }

        private bool LastMoving;
        private void WalkTo()
        {
            if (!Character.IsInRange(Position, 6.0f))
            {
                Character.PositionNoOffset = Position;
                Character.Rotation = Rotation;
            }

            Vector3 predictPosition = Position + (Position - Character.Position) + Velocity;
            float range = predictPosition.DistanceToSquared(Character.Position);

            switch (Speed)
            {
                case 1:
                    if (!Character.IsWalking || range > 0.25f)
                    {
                        float nrange = range * 2;
                        if (nrange > 1.0f)
                        {
                            nrange = 1.0f;
                        }

                        Character.Task.GoStraightTo(predictPosition);
                        Function.Call(Hash.SET_PED_DESIRED_MOVE_BLEND_RATIO, Character.Handle, nrange);
                    }
                    LastMoving = true;
                    break;
                case 2:
                    if (!Character.IsRunning || range > 0.50f)
                    {
                        Character.Task.RunTo(predictPosition, true);
                        Function.Call(Hash.SET_PED_DESIRED_MOVE_BLEND_RATIO, Character.Handle, 1.0f);
                    }
                    LastMoving = true;
                    break;
                case 3:
                    if (!Character.IsSprinting || range > 0.75f)
                    {
                        Function.Call(Hash.TASK_GO_STRAIGHT_TO_COORD, Character.Handle, predictPosition.X, predictPosition.Y, predictPosition.Z, 3.0f, -1, 0.0f, 0.0f);
                        Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER, Character.Handle, 1.49f);
                        Function.Call(Hash.SET_PED_DESIRED_MOVE_BLEND_RATIO, Character.Handle, 1.0f);
                    }
                    LastMoving = true;
                    break;
                default:
                    if (!Character.IsInRange(Position, 0.5f))
                    {
                        Character.Task.RunTo(Position, true, 500);
                    }
                    else if (LastMoving)
                    {
                        Character.Task.StandStill(2000);
                        LastMoving = false;
                    }
                    break;
            }
        }
    }
}
