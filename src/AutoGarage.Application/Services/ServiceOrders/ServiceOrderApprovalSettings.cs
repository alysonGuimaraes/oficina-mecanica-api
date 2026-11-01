namespace AutoGarage.Application.Services.ServiceOrders;

// Prazo, em dias, para responder ao orçamento depois de entrar em AWAITING_APPROVAL.
public record ServiceOrderApprovalSettings(int DeadlineDays);
