# MafDemo.McpClientApp - 檔案結構

以下為 `MafDemo.McpClientApp` 的最新檔案結構（每個檔案後附簡短說明）：

```
MafDemo.McpClientApp
│
├─ `Observability/`
│  ├─ `CorrelationContext.cs`              (追蹤/關聯上下文模型)
│  ├─ `CorrelationIdHandler.cs`            (HTTP handler：注入/轉送 correlation id)
│  ├─ `CorrelationLoggingScope.cs`         (建立 log scope 包含 correlation 資訊)
│  └─ `ToolCallContextAccessor.cs`         (提供 ToolCallContext 的存取/注入)
│
├─ `Audit/`
│  ├─ `IAuditSink.cs`                      (稽核事件接收介面)
│  ├─ `AuditEvent.cs`                      (稽核事件資料模型)
│  ├─ `AuditEventExtensions.cs`            (稽核事件相關延伸方法)
│  ├─ `InMemoryWorkflowDecisionStore.cs`   (記憶體實作的工作流決策儲存)
│  ├─ `InMemoryAuditSink.cs`               (記憶體實作的稽核接收器)
│  ├─ `SerilogAuditSink.cs`                (以 Serilog 發送稽核事件的實作)
│  └─ `CompositeAuditSink.cs`              (組合多個稽核 sink 的實作)
│
├─ `Llm/`
│  ├─ `ILlmClient.cs`                      (LLM 客戶端介面，抽象 LLM 呼叫)
│  ├─ `LlmOptions.cs`                      (LLM 設定選項模型)
│  └─ `OpenAiLlmClient.cs`                 (OpenAI Chat 完成呼叫的具體實作)
│
├─ `Gates/Policy/`
│  ├─ `PolicyGateEvaluator.cs`             (Policy gate 的評估器)
│  ├─ `PolicyGateOptions.cs`               (Policy gate 配置選項)
│  ├─ `PolicyDecisionComposer.cs`          (將 LLM 建議轉為最終決策的組合邏輯)
│  ├─ `PolicyLlmAdvisor.cs`                (與 LLM 互動以取得 policy 建議)
│  ├─ `PolicyLlmPrompt.cs`                 (組成送給 LLM 的 policy 提示字串)
│  ├─ `PolicyLlmSuggestion.cs`             (LLM 建議資料模型)
│  ├─ `IPolicyLlmAdvisor.cs`               (policy LLM 顧問介面)
│  ├─ `PolicyTag.cs`                       (policy 標記/分類模型)
│  ├─ `IPolicyGate.cs`                     (policy gate 介面)
│  ├─ `PolicyDecision.cs`                  (policy 決策資料模型)
│  └─ `PolicyReasonCode.cs`                (policy 決策原因代碼)
│
├─ `Domain/DomainGate/`
│  ├─ `DomainGateEvaluator.cs`             (Domain gate 的評估器)
│  ├─ `DomainGateLlmAdvisor.cs`            (Domain gate 的 LLM 顧問實作)
│  ├─ `DomainGateDecisionPolicy.cs`        (Domain gate 決策政策/規則)
│  ├─ `DomainGateResult.cs`                (Domain gate 評估結果模型)
│  ├─ `DomainClassification.cs`            (領域分類相關模型)
│  └─ `IDomainGate.cs`                     (Domain gate 介面)
│
├─ `Agents/`
│  ├─ `AgentFallbackPolicy.cs`             (Agent 的 fallback 決策策略)
│  ├─ `WorkflowGuard.cs`                   (工作流保護/驗證邏輯)
│  ├─ `WorkflowCapabilityPrompt.cs`        (產生描述工作流能力給 LLM 的提示)
│  ├─ `WorkflowDecision.cs`                (工作流決策資料模型)
│  ├─ `CloudPrintAgent.cs`                 (CloudPrint 相關 agent，協調列印操作)
│  └─ `AgentDecisionResult.cs`             (Agent 決策結果模型)
│
├─ `Adapters/`
│  ├─ `IToolClient.cs`                     (工具客戶端介面)
│  └─ `McpToolClient.cs`                   (MCP API 的工具客戶端實作)
│
├─ `Runtime/`
│  └─ `ToolInvoker.cs`                     (用來呼叫外部工具/adapter 的執行邏輯)
│
├─ `Services/`
│  └─ `ServiceCollectionExtensions.cs`     (註冊 DI services 的擴充方法)
│
├─ `Workflows/`
│  ├─ `GetServerTimeWorkflow.cs`           (範例工作流：取得伺服器時間)
│  ├─ `WorkflowBase.cs`                    (工作流基底類別，提供共用邏輯)
│  ├─ `WorkflowContext.cs`                 (工作流執行時上下文)
│  ├─ `CreatePrintJobWorkflow.cs`          (建立列印工作（PrintJob）的工作流範例)
│  └─ `IWorkflowDefinition.cs`             (工作流定義介面)
│
├─ `Domain/`
│  └─ `ToolCallContext.cs`                 (呼叫工具時使用的領域模型/上下文)
│
├─ `HumanInLoop/`
│  ├─ `IUserConfirmationService.cs`        (人機互動確認服務介面)
│  └─ `ConsoleUserConfirmationService.cs`  (以 Console 提示使用者確認的實作)
│
├─ `Options/`
│  └─ `CloudPrintMcpClientOptions.cs`      (CloudPrint MCP client 相關選項模型)
│
├─ `Policies/`
│  └─ `RetryPolicy.cs`                     (重試政策/策略實作)
│
├─ `Program.cs`                            (應用程式進入點 / 主程式)
│
└─ `obj/` (編譯輸出 / 臨時檔)
   ├─ `Debug/net10.0/MafDemo.McpClientApp.AssemblyInfo.cs`         (編譯產物：組件資訊)
   ├─ `Debug/net10.0/MafDemo.McpClientApp.GlobalUsings.g.cs`       (編譯產物：全域 using)
   └─ `Debug/net10.0/.NETCoreApp,Version=v10.0.AssemblyAttributes.cs` (編譯產物：目標框架屬性)
```

若需把此 Markdown 檔加入 csproj（讓它被包含在專案）或輸出成其他格式，告訴我要怎麼處理即可。