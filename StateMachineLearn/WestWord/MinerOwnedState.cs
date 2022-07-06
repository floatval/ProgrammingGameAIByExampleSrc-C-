﻿namespace StateMachineLearn;
using Behavior = ConstDefine.MinerBehavior;
using Location = ConstDefine.Location;
using EntityManger = GameEntityManger;

/*
 * 矿工状态机
 * 状态机只负责矿工的状态过渡（将矿工的状态从A设置为B - 只是数据层面的改变，不是逻辑层面的改变 - 是逻辑层的前置依赖）处理
 * 负责状态的切换（进入状态、退出状态）由矿工对象的 Update 处理
 */

public sealed class InitState : IState<Miner>
{
    #region Implementation of IState<in Miner>

    /// <summary>
    /// 进入状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Enter(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 将矿工放置到矿洞里面
        owner.CurrentLocation= Location.LocationType.Goldmine;

        WriteExt.WriteBgWhiteAndFgYellow($"MinerId:{entity.InsId}, 初始化状态：进入金矿");
    }

    /// <summary>
    ///  运行状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Execute(Miner owner)
    {
        owner.FSM.ChangState(new EnterMineAndDigForNuggetState()); 
    }

    /// <summary>
    /// 退出状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Exit(Miner owner)
    {
    }

    #endregion
}

/// <summary>
/// 进入金矿挖矿的状态
/// </summary>
public sealed class EnterMineAndDigForNuggetState : IState<Miner>
{
    #region Overrides of MinerState

    /// <summary>
    /// 进入状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Enter(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 已经在矿洞里了，不进行额外的处理
        if (owner.CurrentLocation == Location.LocationType.Goldmine)
        {
            return;
        }
        owner.CurrentLocation= Location.LocationType.Goldmine;

        // 2. 不在矿洞里面则进行移动到矿洞的操作
        WriteExt.WriteBgWhiteAndFgYellow($"MinerId:{entity.InsId}, EnterMineAndDigForNuggetState，进入金矿");
    }

    /// <summary>
    ///  运行状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Execute(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 增加身上的金子
        owner.CurrentGoldCarried++;
        
        // 2. 增加疲劳度
        owner.CurrentTirednessThreshold++;

        // 3. 增加饥渴度
        owner.CurrentThirstLevel++;
        
        WriteExt.WriteBgWhiteAndFgBlue($"MinerId:{owner.InsId}, EnterMineAndDigForNuggetState，挖到了金子");
        
        // 3. 判断背包是否满了
        if (owner.IsPocketsFull())
        {
            owner.FSM.ChangState(VisitBankAndDepositGoldState.Instance);
        }
        
        // 4. 判断饥渴度是否达到了上限
        if (owner.IsThirsty())
        {
            owner.FSM.ChangState(QuenchThirstState.Instance);
        }
        
        // 5. 判断疲劳值是否达到上限
        if (owner.IsFatigued())
        {
            owner.FSM.ChangState(GoHomeAndSleepTilRestedState.Instance);
        }
    }

    /// <summary>
    /// 退出状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Exit(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        WriteExt.WriteBgWhiteAndFgRed($"MinerId:{owner.InsId}, EnterMineAndDigForNuggetState，状态改变,退出金矿挖矿");
        Console.WriteLine('\n');
    }

    #endregion

    #region Singleton

    private static EnterMineAndDigForNuggetState? m_instance;
    
    /// <summary>
    /// 获取实例
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static EnterMineAndDigForNuggetState Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new EnterMineAndDigForNuggetState();
            }

            return m_instance;
        }
        
    }

    #endregion
}

/// <summary>
/// 去银行存钱的状态
/// </summary>
public sealed class VisitBankAndDepositGoldState : IState<Miner>
{
    #region Overrides of MinerState

    /// <summary>
    /// 进入状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Enter(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 切换矿工的位置
        if(owner.CurrentLocation == Location.LocationType.Bank)
        {
            return;
        }
        owner.CurrentLocation = Location.LocationType.Bank;
        
        WriteExt.WriteBgWhiteAndFgYellow($"MinerId:{owner.InsId}, VisitBankAndDepositGoldState，进入银行");
    }
    
    /// <summary>
    ///  运行状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Execute(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 存钱
        owner.CurrentGoldCarried = 0;
        
        WriteExt.WriteBgWhiteAndFgBlue($"MinerId:{owner.InsId}, VisitBankAndDepositGoldState，正在银行存钱");
        
        // 2. 检查饥渴的状态
        if (owner.IsThirsty())
        {
            owner.FSM.ChangState(QuenchThirstState.Instance);
        }
        
        // 3. 检查疲惫状态
        if (owner.IsFatigued())
        {
            owner.FSM.ChangState(GoHomeAndSleepTilRestedState.Instance);
        }
        
        // 4. 状态正常去挖矿
        owner.FSM.ChangState(EnterMineAndDigForNuggetState.Instance);
    }

    /// <summary>
    /// 退出状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Exit(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        WriteExt.WriteBgWhiteAndFgRed($"MinerId:{owner.InsId}, VisitBankAndDepositGoldState，银行存钱动作完成");
        Console.WriteLine('\n');
    }

    #endregion

    #region Singleton

    /// <summary>
    /// 实例缓存
    /// </summary>
    private static VisitBankAndDepositGoldState? m_instance;
    
    /// <summary>
    /// 获取实例
    /// </summary>
    public static VisitBankAndDepositGoldState Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new VisitBankAndDepositGoldState();
            }
            
            return m_instance;
        }
    }

    #endregion
}

/// <summary>
/// 回家休息的状态
/// </summary>
public sealed class GoHomeAndSleepTilRestedState : IState<Miner>
{
    private GoHomeAndSleepTilRestedState()
    {
       m_instance = this; 
    }
    
    #region Overrides of MinerState

    /// <summary>
    /// 进入状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Enter(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 切换矿工位置
        if(owner.CurrentLocation == Location.LocationType.Home)
        {
            return;
        }
        owner.CurrentLocation = Location.LocationType.Home;
        
        WriteExt.WriteBgWhiteAndFgYellow($"MinerId:{owner.InsId}, GoHomeAndSleepTilRestedState，回到家里");
    }

    /// <summary>
    ///  运行状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Execute(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        owner.CurrentTirednessThreshold--;
        WriteExt.WriteBgWhiteAndFgBlue($"MinerId:{owner.InsId}, GoHomeAndSleepTilRestedState，在家里休息中");
        
        // 1. 检查饥渴状态
        if(owner.IsThirsty())
        {
            owner.FSM.ChangState(QuenchThirstState.Instance);
        }
        
        // 2. 状态正常 - 去挖矿
        if (owner.IsNotFatigued())
        {
            owner.FSM.ChangState(EnterMineAndDigForNuggetState.Instance);
        }
    }

    /// <summary>
    /// 退出状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Exit(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        WriteExt.WriteBgWhiteAndFgRed($"MinerId:{owner.InsId}, GoHomeAndSleepTilRestedState，休息好了");
        Console.WriteLine('\n');
    }

    #endregion

    #region Singleton

    /// <summary>
    /// 对象缓存
    /// </summary>
    private static GoHomeAndSleepTilRestedState? m_instance;
    
    /// <summary>
    /// 实例获取
    /// </summary>
    public static GoHomeAndSleepTilRestedState Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new GoHomeAndSleepTilRestedState();
            }
            
            return m_instance;
        }
    }

    #endregion
}


/// <summary>
/// 解决口渴的状态
/// </summary>
public class QuenchThirstState : IState<Miner>
{
    private QuenchThirstState()
    {
        m_instance = this;
    }
    
    #region Overrides of MinerState

    /// <summary>
    /// 进入状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Enter(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 切换位置
        if(owner.CurrentLocation == Location.LocationType.Saloon)
        {
            return;
        }
        owner.CurrentLocation = Location.LocationType.Saloon;
        
        WriteExt.WriteBgWhiteAndFgYellow($"MinerId:{owner.InsId}, QuenchThirstState，进入酒吧");
    }

    /// <summary>
    ///  运行状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Execute(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        WriteExt.WriteBgWhiteAndFgBlue($"MinerId:{owner.InsId}, QuenchThirstState，正在解决口渴");
        owner.CurrentThirstLevel--;

        // 1. 解决完口渴，去挖矿
        if (owner.IsNotThirsty())
        {
            owner.FSM.ChangState(EnterMineAndDigForNuggetState.Instance);
        }
    }

    /// <summary>
    /// 退出状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Exit(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        WriteExt.WriteBgWhiteAndFgRed($"MinerId:{owner.InsId}, QuenchThirstState，解决了口渴");
        Console.WriteLine('\n');
    }

    #endregion
    
    #region Singleton

    /// <summary>
    /// 单例
    /// </summary>
    private static QuenchThirstState? m_instance;
    
    /// <summary>
    /// 获取单例接口
    /// </summary>
    public static QuenchThirstState Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new QuenchThirstState();
            }
            
            return m_instance;
        }
    }
    
    #endregion
}

public class MinerGlobalState : IState<Miner>
{
    #region Implementation of IState<in Miner>

    /// <summary>
    /// 进入状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Enter(Miner owner)
    {
        var entity = EntityManger.Instance.TryGetEntity(owner.InsId);
        if(entity == null)
        {
            return;
        }
        
        // 1. 切换位置
        if(owner.CurrentLocation == Location.LocationType.Saloon)
        {
            return;
        }
        owner.CurrentLocation = Location.LocationType.Saloon;
        
        WriteExt.WriteBgWhiteAndFgYellow($"MinerId:{owner.InsId}, QuenchThirstState，进入酒吧");
    }

    /// <summary>
    ///  运行状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Execute(Miner owner)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 退出状态处理流程
    /// </summary>
    /// <param name="owner"></param>
    public void Exit(Miner owner)
    {
        throw new NotImplementedException();
    }

    #endregion
}