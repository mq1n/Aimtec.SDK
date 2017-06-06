﻿namespace Aimtec.SDK.Prediction.Skillshots
{
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Util;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    // todo move dash to seperate class?
    internal class PredictionImpl
    {
        private struct Path
        { 
                public float Time { get; set; }
                public Vector3 Position { get; set; }
        }

        private struct Dash
        {
            public Vector3 StartPosition { get; set; }
            public Vector3 EndPosition { get; set; }
            public float Speed { get; set; }
            public float StartTime { get; set; }
            public float EndTime { get; set; }
            public float EndTime2 { get; set; }
            public bool IsBlink { get; set; }
            public float Duration { get; set; }
        }

        private struct WaypointInfo
        {
            public Vector3 UnitPosition { get; set; }
            public Vector3 Waypoint { get; set; }
            public float Time { get; set; }
            public int N { get; set; }

        }

        private struct SpellInfo
        {
            public string Name { get; set; }
            public float Duration { get; set; }
        }

        private struct Blink
        {
            public string Name { get; set; }
            public float Range { get; set; }
            public float Delay { get; set; }
            public float Delay2 { get; set; }
        }

        private struct DashResult
        {
            public bool TargetDashing { get; set; }
            public bool CanHit { get; set; }
            public Vector3 Position { get; set; }

            public static bool operator true(DashResult x)
            {
                return x.TargetDashing;
            }

            public static bool operator false(DashResult x)
            {
                return !x.TargetDashing;
            }
        }

        private struct ImmobileResult
        {
            public bool Immobile { get; set; }
            public Vector3 UnitPosition { get; set; }
            public Vector3 CastPosition { get; set; }
        }

        private struct CalculatedTargetPosition
        {
            public Vector3 UnitPosition { get; set; }
            public Vector3 CastPosition { get; set; }
        }

        #region Spell Data

        private List<SpellInfo> Blacklist { get; } =
            new List<SpellInfo>() { new SpellInfo() { Name = "aatroxq", Duration = 0.75f } };

        private List<SpellInfo> DashAboutToHappen { get; } = new List<SpellInfo>()
        {
            new SpellInfo() { Name = "ahritumble", Duration = 0.25f }, //ahri's r
            new SpellInfo() { Name = "akalishadowdance", Duration = 0.25f }, //akali r
            new SpellInfo() { Name = "headbutt", Duration = 0.25f }, //alistar w
            new SpellInfo() { Name = "caitlynentrapment", Duration = 0.25f }, //caitlyn e
            new SpellInfo() { Name = "carpetbomb", Duration = 0.25f }, //corki w
            new SpellInfo() { Name = "dianateleport", Duration = 0.25f }, //diana r
            new SpellInfo() { Name = "fizzpiercingstrike", Duration = 0.25f }, //fizz q
            new SpellInfo() { Name = "fizzjump", Duration = 0.25f }, //fizz e
            new SpellInfo() { Name = "gragasbodyslam", Duration = 0.25f }, //gragas e
            new SpellInfo() { Name = "gravesmove", Duration = 0.25f }, //graves e
            new SpellInfo() { Name = "ireliagatotsu", Duration = 0.25f }, //irelia q
            new SpellInfo() { Name = "jarvanivdragonstrike", Duration = 0.25f }, //jarvan q
            new SpellInfo() { Name = "jaxleapstrike", Duration = 0.25f }, //jax q
            new SpellInfo() { Name = "khazixe", Duration = 0.25f }, //khazix e and e evolved
            new SpellInfo() { Name = "leblancslide", Duration = 0.25f }, //leblanc w
            new SpellInfo() { Name = "leblancslidem", Duration = 0.25f }, //leblanc w (r)
            new SpellInfo() { Name = "blindmonkqtwo", Duration = 0.25f }, //lee sin q
            new SpellInfo() { Name = "blindmonkwone", Duration = 0.25f }, //lee sin w
            new SpellInfo() { Name = "luciane", Duration = 0.25f }, //lucian e
            new SpellInfo() { Name = "maokaiunstablegrowth", Duration = 0.25f }, //maokai w
            new SpellInfo() { Name = "nocturneparanoia2", Duration = 0.25f }, //nocturne r
            new SpellInfo() { Name = "pantheon_leapbash", Duration = 0.25f }, //pantheon e
            new SpellInfo() { Name = "renektonsliceanddice", Duration = 0.25f }, //renekton e
            new SpellInfo() { Name = "riventricleave", Duration = 0.25f }, //riven q
            new SpellInfo() { Name = "rivenfeint", Duration = 0.25f }, //riven e
            new SpellInfo() { Name = "sejuaniarcticassault", Duration = 0.25f }, //sejuani q
            new SpellInfo() { Name = "shenshadowdash", Duration = 0.25f }, //shen e
            new SpellInfo() { Name = "shyvanatransformcast", Duration = 0.25f }, //shyvana r
            new SpellInfo() { Name = "rocketjump", Duration = 0.25f }, //tristana w
            new SpellInfo() { Name = "slashcast", Duration = 0.25f }, //tryndamere e
            new SpellInfo() { Name = "vaynetumble", Duration = 0.25f }, //vayne q
            new SpellInfo() { Name = "viq", Duration = 0.25f }, //vi q
            new SpellInfo() { Name = "monkeykingnimbus", Duration = 0.25f }, //wukong q
            new SpellInfo() { Name = "xenzhaosweep", Duration = 0.25f }, //xin xhao q
            new SpellInfo() { Name = "yasuodashwrapper", Duration = 0.25f }, //yasuo e

        };

        private List<SpellInfo> Spells { get; } = new List<SpellInfo>()
        {
            new SpellInfo() { Name = "katarinar", Duration = 1f }, //Katarinas R
            new SpellInfo() { Name = "drain", Duration = 1f }, //Fiddle W
            new SpellInfo() { Name = "crowstorm", Duration = 1f }, //Fiddle R
            new SpellInfo() { Name = "consume", Duration = 0.5f }, //Nunu Q
            new SpellInfo() { Name = "absolutezero", Duration = 1f }, //Nunu R
            new SpellInfo() { Name = "rocketgrab", Duration = 0.5f }, //Blitzcrank Q
            new SpellInfo() { Name = "staticfield", Duration = 0.5f }, //Blitzcrank R
            new SpellInfo() { Name = "cassiopeiapetrifyinggaze", Duration = 0.5f }, //Cassio's R
            new SpellInfo() { Name = "ezrealtrueshotbarrage", Duration = 1f }, //Ezreal's R
            new SpellInfo() { Name = "galioidolofdurand", Duration = 1f }, //Ezreal's Rg
            new SpellInfo() { Name = "luxmalicecannon", Duration = 1f }, //Lux R
            new SpellInfo() { Name = "reapthewhirlwind", Duration = 1f }, //Jannas R
            new SpellInfo() { Name = "jinxw", Duration = 0.6f }, //jinxW
            new SpellInfo() { Name = "jinxr", Duration = 0.6f }, //jinxR
            new SpellInfo() { Name = "missfortunebullettime", Duration = 1f }, //MissFortuneR
            new SpellInfo() { Name = "shenstandunited", Duration = 1f }, //ShenR
            new SpellInfo() { Name = "threshe", Duration = 0.4f }, //ThreshE
            new SpellInfo() { Name = "threshrpenta", Duration = 0.75f }, //ThreshR
            new SpellInfo() { Name = "infiniteduress", Duration = 1f }, //Warwick R
            new SpellInfo() { Name = "meditate", Duration = 1 }//yi W
        };

        private List<Blink> Blinks { get; } = new List<Blink>()
        {
            new Blink() { Name = "ezrealarcaneshift", Range = 475, Delay = 0.25f, Delay2 = 0.8f }, //Ezreals E
            new Blink() { Name = "deceive", Range = 400, Delay = 0.25f, Delay2 = 0.8f }, //Shacos Q
            new Blink() { Name = "riftwalk", Range = 700, Delay = 0.25f, Delay2 = 0.8f }, //KassadinR
            new Blink() { Name = "gate", Range = 5500, Delay = 1.5f, Delay2 = 1.5f }, //Twisted fate R
            new Blink() { Name = "katarinae", Range = float.MaxValue, Delay = 0.25f, Delay2 = 0.8f }, //Katarinas E
            new Blink()
            {
                Name = "elisespideredescent",
                Range = float.MaxValue,
                Delay = 0.25f,
                Delay2 = 0.8f
            }, //Elise E
            new Blink() { Name = "elisespidere", Range = float.MaxValue, Delay = 0.25f, Delay2 = 0.8f } //Elise insta E
        };

        #endregion

        public PredictionImpl()
        {
            var heroes = ObjectManager.Get<Obj_AI_Hero>();
            var objAiHeroes = heroes as Obj_AI_Hero[] ?? heroes.ToArray();

            this.PathAnalysis = objAiHeroes.ToDictionary(x => x.NetworkId, x => new List<Path>());
            this.DontShootUntilNewWaypoints = objAiHeroes.ToDictionary(x => x.NetworkId, x => false);
            this.TargetsWaypoints = objAiHeroes.ToDictionary(x => x.NetworkId, x => new List<WaypointInfo>());
        
            Obj_AI_Base.OnNewPath += this.OnNewPath;
            Game.OnUpdate += this.OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (sender.Type == GameObjectType.AIHeroClient)
            {
                foreach (var spell in this.Spells)
                {
                    if (e.SpellData.Name.ToLower() == spell.Name)
                    {
                        this.TargetsImmobile[sender.NetworkId] = this.GetTime() + spell.Duration;
                        return;
                    }
                }

                foreach (var spell in this.Blinks)
                {
                    if (e.SpellData.Name.ToLower() == spell.Name)
                    {
                        var landingPos = Vector3.Distance(sender.Position, e.End) < spell.Range
                            ? e.End
                            : sender.Position + spell.Range * Vector3.Normalize(e.End - sender.Position);

                        this.TargetsDashing[sender.NetworkId] = new Dash()
                        {
                            IsBlink = true,
                            Duration = spell.Delay,
                            EndTime = this.GetTime() + spell.Delay,
                            EndTime2 = this.GetTime() + spell.Delay2,
                            StartPosition = sender.Position,
                            EndPosition = landingPos
                        };
                        return;
                    }
                }

                foreach (var spell in this.Blacklist)
                {
                    if (e.SpellData.Name.ToLower() == spell.Name)
                    {
                        this.DontShoot[sender.NetworkId] = this.GetTime() + spell.Duration;
                        return;
                    }
                }

                foreach (var spell in this.DashAboutToHappen)
                {
                    if (e.SpellData.Name.ToLower() == spell.Name)
                    {
                        this.DontShoot2[sender.NetworkId] = this.GetTime() + spell.Duration;
                    }
                }
            }
        }

        private ImmobileResult IsImmobile(
            Obj_AI_Base unit,
            float delay,
            float radius,
            float speed,
            Vector3 from,
            SkillType spellType)
        {
            if (this.TargetsImmobile.ContainsKey(unit.NetworkId))
            {
                var extraDelay = Math.Abs(speed - float.MaxValue) < float.Epsilon ? 0 : Vector3.Distance(from, unit.Position) / speed;

                if (this.TargetsImmobile[unit.NetworkId] > this.GetTime() + delay + extraDelay
                    && spellType == SkillType.Circle)
                {
                    return new ImmobileResult()
                    {
                        Immobile = true,
                        UnitPosition = unit.Position,
                        CastPosition = unit.Position + radius / 3f * Vector3.Normalize(from - unit.Position)
                    };
                }

                if (this.TargetsImmobile[unit.NetworkId] + radius / unit.MoveSpeed
                    > this.GetTime() + delay + extraDelay)
                {
                    return new ImmobileResult()
                    {
                        Immobile = true,
                        UnitPosition = unit.Position,
                        CastPosition = unit.Position
                    };
                }
            }

            return new ImmobileResult(){Immobile = false, UnitPosition = unit.Position, CastPosition = unit.Position};
        }

        private DashResult IsDashing(Obj_AI_Base unit, float delay, float radius, float speed, Vector3 from)
        {
            var targetDashing = false;
            var canHit = false;
            var position = Vector3.Zero;

            if (this.TargetsDashing.ContainsKey(unit.NetworkId))
            {
                var dash = this.TargetsDashing[unit.NetworkId];

                if (dash.EndTime >= this.GetTime())
                {
                    targetDashing = true;

                    if (dash.IsBlink)
                    {
                        if (dash.EndTime - this.GetTime() <= delay + Vector3.Distance(from, dash.EndPosition) / speed)
                        {
                            position = dash.EndPosition;
                            canHit = (unit.MoveSpeed * (delay + Vector3.Distance(from, dash.EndPosition) / speed
                                - (dash.EndTime2 - this.GetTime()))) < radius;
                        }

                        if (dash.EndTime - this.GetTime()
                            >= (delay + Vector3.Distance(from, dash.StartPosition) / speed) && !canHit)
                        {
                            position = dash.EndPosition;
                            canHit = true;
                        }
                    }
                    else
                    {
                        var vmc = Geometry.VectorMovementCollision(
                            (Vector2) dash.StartPosition,
                            (Vector2) dash.EndPosition,
                            dash.Speed,
                            (Vector2) @from,
                            speed,
                            this.GetTime() - dash.StartTime + delay);

                        if (Math.Abs(vmc.Time) > float.Epsilon) // TODO Check if this is correct
                        {
                            position = new Vector3(vmc.Position.X, 0, vmc.Position.Y);
                            canHit = true;
                        }
                        else
                        {
                            position = dash.EndPosition;
                            canHit = unit.MoveSpeed * (delay + Vector3.Distance(from, position) / speed
                                - (dash.EndTime - this.GetTime())) < radius;
                        }
                    }
                }
            }

            return new DashResult(){TargetDashing = targetDashing, CanHit = canHit, Position = position};
        }

        private float lastTick = float.NaN;

        // Store waypoints for 10 seconds
        private const float WaypointsTime = 10;

        private void OnUpdate()
        {
            // update every 2/10 of a second
            if (float.IsNaN(this.lastTick) || this.GetTime() - this.lastTick > 2 / 10f)
            {
                this.lastTick = this.GetTime();

                // update the path anaylsis of enemies
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                {
                    var paths = this.PathAnalysis[enemy.NetworkId];

                    foreach (var path in paths.ToArray())
                    {
                        if (this.GetTime() - 1.5 > path.Time)
                        {
                            paths.Remove(path);
                        }
                    }
                }

                // update the players analysis
                var playerPaths = this.PathAnalysis[Player.NetworkId];
                foreach (var path in playerPaths.ToArray())
                {
                    if (this.GetTime() - 1.5 > path.Time)
                    {
                        playerPaths.Remove(path);
                    }
                }

                foreach (var targetWaypoints in this.TargetsWaypoints)
                {
                    // key   - networkid
                    // value - list of waypoints
                    foreach (var waypoint in targetWaypoints.Value.ToArray())
                    {
                        if (waypoint.Time + WaypointsTime < this.GetTime())
                        {
                            targetWaypoints.Value.Remove(waypoint);
                        }
                    }
                }
            }
        }

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        private Dictionary<int, List<Path>> PathAnalysis { get; }
        private Dictionary<int, bool> DontShootUntilNewWaypoints { get; }
        private Dictionary<int, List<WaypointInfo>> TargetsWaypoints { get; }
        private Dictionary<int, Dash> TargetsDashing { get; } = new Dictionary<int, Dash>();
        private Dictionary<int, float> TargetsImmobile { get; } = new Dictionary<int, float>();
        private Dictionary<int, float> DontShoot { get; } = new Dictionary<int, float>();
        private Dictionary<int, float> DontShoot2 { get; } = new Dictionary<int, float>();

        private void OnNewPath(Obj_AI_Base sender, Obj_AI_BaseNewPathEventArgs e)
        {
            if (sender.Type == GameObjectType.AIHeroClient && sender.Team != Player.Team
                || sender.NetworkId == Player.NetworkId)
            {
                if (this.PathAnalysis[sender.NetworkId].Count == 2)
                {
                    var p1 = this.PathAnalysis[sender.NetworkId][this.PathAnalysis[sender.NetworkId].Count - 2].Position;
                    var p2 = this.PathAnalysis[sender.NetworkId][this.PathAnalysis[sender.NetworkId].Count - 1].Position;
                    var angle = ((Vector2) sender.Position).AngleBetween((Vector2) p1, (Vector2) p2);

                    if (angle > 20)
                    {
                       this.PathAnalysis[sender.NetworkId].Add(new Path(){Time = Environment.TickCount / 1000f, Position = e.Path.Last()});
                    }
                }
                else
                {
                    this.PathAnalysis[sender.NetworkId].Add(new Path(){Time = Environment.TickCount / 1000f, Position =  e.Path.Last()});
                }
            }

            if (sender.IsValid && sender.Type == GameObjectType.AIHeroClient)
            {
                this.DontShootUntilNewWaypoints[sender.NetworkId] = false;

                var waypointsToAdd = this.GetCurrentWaypoints(sender);

                if(waypointsToAdd.Count >= 1) // todo Maybe 2?
                {
                    this.TargetsWaypoints[sender.NetworkId].Add(
                        new WaypointInfo()
                        {
                            UnitPosition = sender.Position,
                            Waypoint = waypointsToAdd.Last(),
                            Time = this.GetTime(),
                            N = waypointsToAdd.Count
                        });
                }
            }
            else
            {
                // could do healthrped stuff
            }

            if (e.IsDash)
            {
                if (sender.Type == GameObjectType.AIHeroClient)
                {
                    var dash = new Dash()
                    {
                        StartPosition = e.Path[0],
                        EndPosition = e.Path.Last(),
                        Speed = 1000, // todo proper api
                        StartTime = this.GetTime() - Game.Ping / 2000f,
                        
                    };

                    var dist = Vector3.Distance(dash.StartPosition, dash.EndPosition);
                    dash.EndTime = dash.StartTime + (dist / dash.Speed);

                    this.TargetsDashing[sender.NetworkId] = dash;
                    this.DontShootUntilNewWaypoints[sender.NetworkId] = true;
                }
            }         
        }

        private float GetTime()
        {
            return Environment.TickCount / 1000f;
        }

        private List<Vector3> GetCurrentWaypoints(Obj_AI_Base @object)
        {
            // todo  .Path might have the current position, didnt check :Roto2:
            var result = new List<Vector3> { @object.Position };

            if (@object.HasPath)
            {

                result.AddRange(@object.Path);
            }

            return result;
        }

        private PredictionOutput GetLineCastPosition(
            Obj_AI_Base unit,
            float delay,
            float radius,
            float range,
            float speed,
            GameObject from,
            bool collision)
        {
            return this.GetCastPosition(unit, delay, radius, range, speed, from.Position, collision, SkillType.Line);
        }

        private CalculatedTargetPosition CalculateTargetPosition(
            Obj_AI_Base unit,
            float delay,
            float radius,
            float speed,
            Vector3 from,
            SkillType type,
            Vector3 second = new Vector3())
        {
            if (unit.Type == GameObjectType.AIHeroClient && unit.IsEnemy || unit.IsMe)
            {
                if (this.PathAnalysis[unit.NetworkId].Count > 4)
                {
                    return new CalculatedTargetPosition(){CastPosition = unit.Position, UnitPosition = unit.Position};
                }

                if (this.PathAnalysis[unit.NetworkId].Count > 3)
                {
                    delay *= 0.8f;
                    speed *= 1.2f;
                }
            }

            var spot = Vector3.Zero;

            if (unit.IsValidTarget()) // todo eh
            {
                var p90x = second.IsZero ? unit.Position : second;
                var pathPot = unit.MoveSpeed * (Vector3.Distance(Player.Position, p90x) / speed + delay);

                if (unit.Path.Length < 3)
                {
                    var v = unit.Position + Vector3.Normalize(unit.Path.Last() - unit.Position)
                        * (pathPot - unit.BoundingRadius + 10);

                    if (Vector3.Distance(unit.Position, v) > 1)
                    {
                        if (Vector3.Distance(unit.Path.Last(), unit.Position) >= Vector3.Distance(unit.Position, v))
                        {
                            spot = v;
                        }
                        else
                        {
                            spot = unit.Path.Last();
                        }
                    }
                    else
                    {
                        spot = unit.Path.Last();
                    }
                }
                else
                {
                    for (var i = 1; i < unit.Path.Length; i++)
                    {
                        var pStart = unit.Path[i - i];
                        var pEnd = unit.Path[i];
                        var iPathDist = Vector3.Distance(pStart, pEnd);

                        if (pathPot > iPathDist)
                        {
                            pathPot = pathPot - iPathDist;
                        }
                        else
                        {
                            var v = pStart + Vector3.Normalize(pEnd - pStart) * (pathPot - unit.BoundingRadius + 10);
                            spot = v;

                            if (!second.IsZero)
                            {
                                return new CalculatedTargetPosition(){UnitPosition = spot, CastPosition = spot};
                            }
                            else
                            {
                                return this.CalculateTargetPosition(unit, delay, radius, speed, from, type, spot);
                            }
                        }
                    }

                    if (Vector3.Distance(unit.Position, unit.Path.Last()) > unit.BoundingRadius)
                    {
                        spot = unit.Path.Last();
                    }
                    else
                    {
                        spot = unit.Position;
                    }
                }
            }

            spot = spot.IsZero ? unit.Position : spot;

            if (!second.IsZero)
            {
                return new CalculatedTargetPosition(){CastPosition = spot, UnitPosition = spot };
            }

            return this.CalculateTargetPosition(unit, delay, radius, speed, from, type, spot);
        }

        private PredictionOutput GetCastPosition(Obj_AI_Base unit, float delay, float radius, float range, float speed, Vector3 from, bool collision, SkillType type)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            range = Math.Abs(range) < float.Epsilon ? float.MaxValue : range - 15;
            radius = Math.Abs(radius) < float.Epsilon ? 1 : radius + this.GetHitBox(unit) - 4;
            from = from.IsZero ? ObjectManager.GetLocalPlayer().Position : from;

            var isFromPlayer = Vector3.DistanceSquared(from, ObjectManager.GetLocalPlayer().Position) < 50 * 50;
            delay = delay + (0.07f + Game.Ping / 2000f);

            var position = unit.Position;
            var castPosition = unit.Position;
            var hitchance = 0;

            var dashResult = this.IsDashing(unit, delay, radius, speed, from);
            var immobileResult = this.IsImmobile(unit, delay, radius, speed, from, type);

            if (unit.Type != GameObjectType.AIHeroClient)
            {
                var targetPos = this.CalculateTargetPosition(unit, delay, radius, speed, from, type);

                position = targetPos.UnitPosition;
                castPosition = targetPos.CastPosition;
                hitchance = 2;
            }
            else
            {
                if (this.DontShoot[unit.NetworkId] > this.GetTime())
                {
                    position = unit.Position;
                    castPosition = unit.Position;
                    hitchance = 0;

                }
                else if (dashResult)
                {
                    if (dashResult.CanHit)
                    {
                        hitchance = 5;
                    }
                    else
                    {
                        hitchance = 0;
                    }

                    position = dashResult.Position;
                    castPosition = dashResult.Position;
                }
                else if (this.DontShoot2[unit.NetworkId] > this.GetTime())
                {
                    position = unit.Position;
                    castPosition = unit.Position;
                    hitchance = 7;
                }
                else if (immobileResult.Immobile)
                {
                    position = immobileResult.UnitPosition;
                    castPosition = immobileResult.CastPosition;
                    hitchance = 4;
                }
                else
                {
                    var wpa = this.AnalyzeWaypoints(unit, delay, radius, range, speed, from,type);
                    castPosition = wpa.CastPosition;
                    hitchance = (int) wpa.HitChance;
                    position = wpa.PredictedPosition;
                }

            }
            if (isFromPlayer)
            {
                if (type == SkillType.Line && Vector3.DistanceSquared(from, position) >= range * range)
                {
                    hitchance = 0;
                }

                if (type == SkillType.Circle && Vector3.DistanceSquared(from, position) >= Math.Pow(range + radius, 2))
                {
                    hitchance = 0;
                }

                if (Vector3.DistanceSquared(from, castPosition) > range * range)
                {
                    hitchance = 2;
                }
            }

            radius -= this.GetHitBox(unit) + 4;

            // todo collision

            return null;
        }

        private PredictionOutput AnalyzeWaypoints(
            Obj_AI_Base unit,
            float delay,
            float radius,
            float range,
            float speed,
            Vector3 from,
            SkillType type)
        {
            int hitchance;

            var savedWaypoints = this.TargetsWaypoints[unit.NetworkId];
            var currentWaypoints = this.GetCurrentWaypoints(unit);
            var visibleSince = this.GetTime();

            if (delay < 0.25)
            {
                hitchance = 2;
            }
            else
            {
                hitchance = 1;
            }

            var calculatedTargetPosition = this.CalculateTargetPosition(unit, delay, radius, speed, from, type);
            var position = calculatedTargetPosition.UnitPosition;
            var castPosition = calculatedTargetPosition.CastPosition;

            if (this.CountWaypoints(unit.NetworkId, this.GetTime() - 0.1f) >= 1
                || this.CountWaypoints(unit.NetworkId, this.GetTime() - 1) == 1)
            {
                hitchance = 2;
            }

            var n = 2;
            var t1 = 0.5f;

            if (this.CountWaypoints(unit.NetworkId, this.GetTime() - 0.75f) >= n)
            {
                var angle = this.MaxAngle(unit, currentWaypoints.Last(), this.GetTime() - t1);
                if (angle > 90)
                {
                    hitchance = 1;
                }
                else if(angle < 30 && this.CountWaypoints(unit.NetworkId, this.GetTime() -0.1f -t1) >= 1)
                {
                    hitchance = 2;
                }
            }

            n = 1;
            if (this.CountWaypoints(unit.NetworkId, this.GetTime() - n) == 0)
            {
                hitchance = 2;
            }
            
            // todo p fm
            if (currentWaypoints.Count <= 1 && this.GetTime() - visibleSince > 1)
            {
                hitchance = 2;
            }

            // todo slowed

            if(!position.IsZero && !castPosition.IsZero && ((radius / unit.MoveSpeed >= delay + Vector3.Distance(from, castPosition)/speed) || (radius / unit.MoveSpeed) >= delay + Vector3.Distance(from, position)/speed))
            {
                hitchance = 3;
            }

            if (((Vector2) from).AngleBetween((Vector2) unit.Position, (Vector2) castPosition) > 60)
            {
                hitchance = 1;
            }

            if (position.IsZero || castPosition.IsZero)
            {
                hitchance = 0;
                castPosition = unit.Position;
                position = castPosition;
            }

            if (Vector3.Distance(Player.Position, unit.Position) < 250 && unit.NetworkId != Player.NetworkId)
            {
                hitchance = 2;
                calculatedTargetPosition = this.CalculateTargetPosition(unit, delay * 0.5f, radius, speed * 2, from, type);
                position = calculatedTargetPosition.UnitPosition;
                castPosition = calculatedTargetPosition.CastPosition;
            }

            if (savedWaypoints.Count == 0 && this.GetTime() - visibleSince > 3)
            {
                hitchance = 2;
            }

            if (this.DontShootUntilNewWaypoints[unit.NetworkId])
            {
                hitchance = 0;
                castPosition = unit.Position;
                position = castPosition;
            }

            return new PredictionOutput(){CastPosition = castPosition, PredictedPosition = position, HitChance = (HitChance) hitchance};

        }

        private float MaxAngle(GameObject unit, Vector3 currentWaypoint, float from)
        {
            return this.GetWaypoints(unit.NetworkId, from)
                .Select(
                    x => Vector2.Zero.AngleBetween(
                        (Vector2) currentWaypoint - (Vector2) unit.Position,
                        (Vector2) (x.Waypoint - x.UnitPosition)))
                .Concat(new[] { 0f }).Max();
        }

        private List<WaypointInfo> GetWaypoints(int networkId, float from, float to = float.NaN)
        {
            return this.TargetsWaypoints[networkId].Where(x => from <= x.Time && (float.IsNaN(to) ? this.GetTime() : to) >= x.Time).ToList();
        }

        private int CountWaypoints(int networkId, float from, float to = float.NaN)
        {
            return this.GetWaypoints(networkId, from, to).Count;
        }

        private float GetHitBox(Obj_AI_Base unit)
        {
            // todo properhitbox
            return 65;
        }
    }
}
