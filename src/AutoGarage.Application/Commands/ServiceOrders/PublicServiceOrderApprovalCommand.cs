namespace AutoGarage.Application.Commands.ServiceOrders;

public record PublicServiceOrderApprovalCommand(string ServiceOrderId, bool? Approved);
