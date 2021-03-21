using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleModel
{
    private static BattleModel _instance;
    public static BattleModel Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BattleModel();
            return _instance;
        }
    }

    public GameCollection<BattleEntity> ActiveBattleEntities;
    public GameCollection<CoverLocation> ActiveCoverEntities;

    private BattleField _battleField;
    public BattleField BattleField { get { return _battleField; } }
    public void SetBattleField(BattleField newBattleField)
    {
        _battleField = newBattleField;
    }

    public BattleModel()
    {
        ActiveBattleEntities = new GameCollection<BattleEntity>();
        ActiveCoverEntities = new GameCollection<CoverLocation>();
    }
}
