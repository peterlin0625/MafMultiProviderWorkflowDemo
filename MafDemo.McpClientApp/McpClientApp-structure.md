# MafDemo.McpClientApp - 檔案結構

以下為 `MafDemo.McpClientApp` 的檔案結構（每個檔案後附簡短說明）：

```
MafDemo.McpClientApp
│
├─ `Observability/`
│  ├─ `CorrelationContext.cs`              (追蹤/關聯上下文模型)
│  ├─ `CorrelationIdHandler.cs`            (HTTP handler：注入/轉送 correlation id)
│  └─ `CorrelationLoggingScope.cs`         (建立 log scope 包含 correlation 資訊)
│
├─ `Audit/`
│  ├─ `IWorkflowDecisionStore.cs`          (工作流決策儲存介面)
│  ├─ `InMemoryWorkflowDecisionStore.cs`   (記憶體實作的決策儲存)
│  ├─ `IWorkflowAuditSink.cs`              (稽核事件接收介面)
│  ├─ `InMemoryWorkflowAuditSink.cs`       (記憶體實作的稽核接收器)
│  └─ `WorkflowAuditEvent.cs`              (工作流稽核事件資料模型)
│
├─ `Llm/`
│  ├─ `ILlmClient.cs`                      (LLM 客戶端介面)
│  ├─ `LlmOptions.cs`                      (LLM 設定選項模型)
│  └─ `OpenAiLlmClient.cs`                 (OpenAI Chat 完成呼叫的實作)
│
├─ `Agents/`
│  ├─ `AgentFallbackPolicy.cs`             (Agent fallback 決策策略)
│  ├─ `WorkflowGuard.cs`                   (工作流保護/驗證邏輯)
│  ├─ `WorkflowCapabilityPrompt.cs`        (建立提示描述工作流能力給 LLM)
│  ├─ `WorkflowDecision.cs`                (工作流決策資料模型)
│  ├─ `CloudPrintAgent.cs`                 (CloudPrint 相關的 agent 實作)
│  └─ `AgentDecisionResult.cs`             (Agent 決策結果模型)
│
├─ `Runtime/`
│  └─ `ToolInvoker.cs`                     (呼叫外部工具/Adapter 的執行邏輯)
│
├─ `Adapters/`
│  ├─ `IToolClient.cs`                     (工具客戶端介面)
│  └─ `McpToolClient.cs`                   (MCP API 的工具客戶端實作)
│
├─ `Services/`
│  └─ `ServiceCollectionExtensions.cs`     (DI 註冊擴充方法)
│
├─ `Workflows/`
│  ├─ `GetServerTimeWorkflow.cs`           (範例工作流：取得伺服器時間)
│  ├─ `WorkflowBase.cs`                    (工作流基底類別)
│  ├─ `WorkflowContext.cs`                 (工作流執行上下文)
│  ├─ `CreatePrintJobWorkflow.cs`          (建立列印工作流程範例)
│  └─ `IWorkflowDefinition.cs`             (工作流定義介面)
│
├─ `Domain/`
│  └─ `ToolCallContext.cs`                 (呼叫工具的領域模型/上下文)
│
├─ `HumanInLoop/`
│  ├─ `IUserConfirmationService.cs`        (人機互動確認服務介面)
│  └─ `ConsoleUserConfirmationService.cs`  (Console 互動式確認實作)
│
├─ `Options/`
│  └─ `CloudPrintMcpClientOptions.cs`      (CloudPrint MCP client 選項模型)
│
├─ `Policies/`
│  └─ `RetryPolicy.cs`                     (重試策略實作)
│
├─ `Program.cs`                            (應用程式進入點)
│
└─ `obj/` (編譯輸出 / 臨時檔)
   ├─ `Debug/net10.0/MafDemo.McpClientApp.AssemblyInfo.cs`         (編譯產物：組件資訊)
   ├─ `Debug/net10.0/MafDemo.McpClientApp.GlobalUsings.g.cs`       (編譯產物：全域 using)
   └─ `Debug/net10.0/.NETCoreApp,Version=v10.0.AssemblyAttributes.cs` (編譯產物：目標框架屬性)
```

需要我把此檔加入到專案 csproj 或以其它格式輸出嗎？