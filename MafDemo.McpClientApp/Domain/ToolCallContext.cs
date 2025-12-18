using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Domain;

/// <summary>
/// 表示工具呼叫的目前生命週期狀態。
/// </summary>
public enum ToolCallStatus
{
    /// <summary>工具呼叫已建立但尚未嘗試。</summary>
    Created,

    /// <summary>工具呼叫正在進行中。</summary>
    InProgress,

    /// <summary>工具呼叫已成功完成。</summary>
    Succeeded,

    /// <summary>工具呼叫發生錯誤。</summary>
    Failed,

    /// <summary>工具呼叫已被中止，且不會重試。</summary>
    Aborted
}

/// <summary>
/// 保存單一工具呼叫的執行期間上下文。
/// 追蹤識別、意圖（副作用/冪等）、嘗試次數、狀態與錯誤。
/// </summary>
public sealed class ToolCallContext
{
    /// <summary>工具呼叫實例的唯一識別碼。</summary>
    public string ToolCallId { get; }

    /// <summary>被呼叫工具的邏輯名稱。</summary>
    public string ToolName { get; }

    /// <summary>若為 true，表示此工具會產生外部狀態變更（副作用）。</summary>
    public bool IsSideEffect { get; }

    /// <summary>
    /// 若為 true，表示呼叫方期望該操作為冪等，對重試邏輯有參考價值。
    /// </summary>
    public bool IdempotencyExpected { get; }

    /// <summary>已對此工具呼叫嘗試的次數。</summary>
    public int AttemptCount { get; private set; }

    /// <summary>工具呼叫的當前狀態。</summary>
    public ToolCallStatus Status { get; private set; }

    /// <summary>最近一次失敗時記錄的例外（若有）。</summary>
    public Exception? LastError { get; private set; }

    /// <summary>ToolCallContext 建立時間（UTC）。</summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 建立新的 <see cref="ToolCallContext"/>。
    /// </summary>
    /// <param name="toolCallId">此呼叫的唯一識別碼。</param>
    /// <param name="toolName">被呼叫的工具名稱。</param>
    /// <param name="isSideEffect">此呼叫是否會產生副作用。</param>
    /// <param name="idempotencyExpected">呼叫方是否期望該操作為冪等。</param>
    public ToolCallContext(
        string toolCallId,
        string toolName,
        bool isSideEffect,
        bool idempotencyExpected)
    {
        ToolCallId = toolCallId;
        ToolName = toolName;
        IsSideEffect = isSideEffect;
        IdempotencyExpected = idempotencyExpected;
        Status = ToolCallStatus.Created;
    }

    /// <summary>
    /// 記錄一次嘗試。會遞增 <see cref="AttemptCount"/> 並將 <see cref="Status"/> 設為 <see cref="ToolCallStatus.InProgress"/>。
    /// </summary>
    public void MarkAttempt()
    {
        AttemptCount++;
        Status = ToolCallStatus.InProgress;
    }

    /// <summary>將工具呼叫標記為成功。</summary>
    public void MarkSuccess()
    {
        Status = ToolCallStatus.Succeeded;
    }

    /// <summary>
    /// 記錄工具呼叫的失敗並儲存例外資訊。
    /// </summary>
    /// <param name="ex">導致失敗的例外。</param>
    public void MarkFailure(Exception ex)
    {
        LastError = ex;
        Status = ToolCallStatus.Failed;
    }

    /// <summary>中止工具呼叫；將狀態設為 <see cref="ToolCallStatus.Aborted"/>。</summary>
    public void Abort()
    {
        Status = ToolCallStatus.Aborted;
    }
}

