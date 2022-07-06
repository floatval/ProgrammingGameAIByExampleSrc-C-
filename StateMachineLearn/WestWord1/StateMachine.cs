﻿namespace StateMachineLearn;

/// <summary>
/// 管理状态迁移的状态机
/// </summary>
/// <typeparam name="TOwner">状态机的拥有者的类型-实体类（非接口）</typeparam>
public class StateMachine<TOwner> where TOwner: class
{
   public StateMachine(TOwner owner, IState<TOwner> currentState, IState<TOwner> previousState)
   {
      Owner = owner;
      CurrentState = currentState;
      PreviousState = previousState;
   }
   
   /// <summary>
   /// 拥有者
   /// </summary>
   public TOwner Owner { get; private set; }
   
   /// <summary>
   /// 拥有者当前状态
   /// </summary>
   public IState<TOwner> CurrentState { get; set; }
   
   /// <summary>
   ///  拥有者上一个状态 
   /// </summary>
   public IState<TOwner> PreviousState { get; set; }
   
   /// <summary>
   /// 拥有者全局状态
   /// </summary>
   public IState<TOwner>? GlobalState { get; set; }
   
   /// <summary>
   /// 更新状态 - 每次主循环调用
   /// </summary>
   public void Update()
   {
      // 1. 检查全局状态
      GlobalState?.Execute(Owner);
      
      // 2. 运行当前状态
      CurrentState.Execute(Owner);
   }
   
   /// <summary>
   /// 更改状态
   /// </summary>
   /// <param name="state"></param>
   /// <returns></returns>
   public bool ChangState(IState<TOwner> state)
   {
      // 1. 如果传入的状态和当前状态一致，则返回false
      if(ReferenceEquals(CurrentState, state))
      {
         return false;
      }
      
      // 2. 退出之前的状态
      CurrentState.Exit(Owner);
      
      // 3. 更新上一次的状态
      PreviousState = CurrentState;
      
      // 4. 更新当前状态
      CurrentState = state;
      
      // 5. 进入当前状态
      CurrentState.Enter(Owner);
      
      return true;
   }

   /// <summary>
   /// 回退到之前的状态
   /// </summary>
   /// <returns></returns>
   public bool RevertToPreviousState()
   {
      return ChangState(PreviousState);
   }
   
   /// <summary>
   /// 是否已经处于某个状态
   /// </summary>
   /// <param name="state"></param>
   /// <returns></returns>
   public bool IsInState(IState<TOwner> state)
   {
      return ReferenceEquals(CurrentState, state);
   }
   
}