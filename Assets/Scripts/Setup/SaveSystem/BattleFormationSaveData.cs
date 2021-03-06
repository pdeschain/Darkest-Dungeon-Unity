﻿using System.IO;
using System.Collections.Generic;

public class BattleFormationSaveData
{
    public List<FormationUnitSaveData> unitData;

    public BattleFormationSaveData()
    {
        unitData = new List<FormationUnitSaveData>();
    }

    public void UpdateFormation(BattleFormation formation)
    {
        unitData.Clear();
        for(int i = 0; i < formation.party.Units.Count; i++)
        {
            var newFormationUnitData = new FormationUnitSaveData();
            newFormationUnitData.UpdateFromUnit(formation.party.Units[i]);
            unitData.Add(newFormationUnitData);
        }
    }

    public void WriteFormationData(BinaryWriter bw)
    {
        bw.Write(unitData.Count);
        for (int i = 0; i < unitData.Count; i++)
            unitData[i].WriteFormationUnitData(bw);
    }
    public void ReadFormationData(BinaryReader br)
    {
        int unitDataCount = br.ReadInt32();
        unitData.Clear();
        for (int i = 0; i < unitDataCount; i++)
        {
            var newFormationUnitData = new FormationUnitSaveData();
            newFormationUnitData.ReadFormationUnitData(br);
            unitData.Add(newFormationUnitData);
        }
    }
}

public class FormationUnitSaveData
{
    public bool IsHero { get; set; }
    public int Rank { get; set; }
    public Team Team { get; set; }

    public int RosterId { get; set; }
    public string CurrentMode { get; set; }

    public bool IsCorpse { get; set; }
    public string OriginalClass { get; set; }
    public string Class { get; set; }
    public string Name { get; set; }
    public float CurrentHp { get; set; }

    public List<BuffInfo> Buffs { get; set; }
    public Dictionary<StatusType, StatusEffect> Statuses { get; set; }

    public FormationUnitInfo CombatInfo { get; set; }

    public FormationUnitSaveData()
    {
        Buffs = new List<BuffInfo>();
        Statuses = new Dictionary<StatusType, StatusEffect>();
        CurrentMode = "";
        OriginalClass = "";
    }

    public void UpdateFromUnit(FormationUnit unit)
    {
        Rank = unit.Rank;
        Team = unit.Team;
        CombatInfo = unit.CombatInfo;
        CurrentMode = unit.Character.Mode == null ? "" : unit.Character.Mode.Id;
        OriginalClass = unit.OriginalClass;
        IsCorpse = unit.IsCorpse;

        unit.Character.UpdateSaveData(this);
    }

    public void WriteFormationUnitData(BinaryWriter bw)
    {
        bw.Write(IsHero);
        bw.Write(Rank);
        bw.Write((int)Team);

        bw.Write(Statuses.Count);
        foreach(var status in Statuses.Values)
        {
            bw.Write((int)status.Type);
            status.WriteStatusData(bw);
        }

        if (IsHero)
        {
            bw.Write(RosterId);
            bw.Write(CurrentMode);
        }
        else
        {
            bw.Write(IsCorpse);
            bw.Write(OriginalClass);
            bw.Write(Class);
            bw.Write(Name);
            bw.Write(CurrentHp);

            bw.Write(Buffs.Count);
            for (int i = 0; i < Buffs.Count; i++)
            {
                bw.Write((int)Buffs[i].SourceType);

                if (Buffs[i].SourceType == BuffSourceType.Adventure)
                {
                    bw.Write((int)Buffs[i].DurationType);
                    bw.Write(Buffs[i].OverridenValue);
                    bw.Write(Buffs[i].Duration);

                    bw.Write(Buffs[i].Buff.Id);
                    if (Buffs[i].Buff.Id == "")
                    {
                        bw.Write(Buffs[i].Buff.ModifierValue);
                        bw.Write((int)Buffs[i].Buff.Type);
                        bw.Write((int)Buffs[i].Buff.AttributeType);
                        bw.Write((int)Buffs[i].Buff.RuleType);
                    }
                }
            }
        }

        CombatInfo.WriteCombatInfoData(bw);
    }
    public void ReadFormationUnitData(BinaryReader br)
    {
        IsHero = br.ReadBoolean();
        Rank = br.ReadInt32();
        Team = (Team)br.ReadInt32();

        int statusCount = br.ReadInt32();
        Character.InitializeBasicStatuses(Statuses);
        for(int i = 0; i < statusCount; i++)
        {
            var savedStatusType = (StatusType)br.ReadInt32();
            Statuses[savedStatusType].ReadStatusData(br);
        }

        if (IsHero)
        {
            RosterId = br.ReadInt32();
            CurrentMode = br.ReadString();
        }
        else
        {
            IsCorpse = br.ReadBoolean();
            OriginalClass = br.ReadString();
            Class = br.ReadString();
            Name = br.ReadString();
            CurrentHp = br.ReadSingle();

            int buffEntryCount = br.ReadInt32();
            Buffs.Clear();
            for (int i = 0; i < buffEntryCount; i++)
            {
                BuffSourceType buffSourceType = (BuffSourceType)br.ReadInt32();

                if (buffSourceType == BuffSourceType.Adventure)
                {
                    BuffInfo newBuffInfo = new BuffInfo((BuffDurationType)br.ReadInt32(), buffSourceType);

                    newBuffInfo.OverridenValue = br.ReadSingle();
                    newBuffInfo.Duration = br.ReadInt32();

                    string buffId = br.ReadString();
                    if (buffId == "")
                    {
                        newBuffInfo.Buff = new Buff()
                        {
                            Id = "",
                            ModifierValue = br.ReadSingle(),
                            Type = (BuffType)br.ReadInt32(),
                            AttributeType = (AttributeType)br.ReadInt32(),
                            RuleType = (BuffRule)br.ReadInt32(),
                        };
                    }
                    else
                        newBuffInfo.Buff = DarkestDungeonManager.Data.Buffs[buffId];

                    Buffs.Add(newBuffInfo);
                }
            }
        }

        CombatInfo = new FormationUnitInfo();
        CombatInfo.ReadCombatInfoData(br);
    }
}