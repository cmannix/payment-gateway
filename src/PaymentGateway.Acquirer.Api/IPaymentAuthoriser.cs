﻿using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Acquirer.Api
{
    public interface IPaymentAuthoriser
    {
        Task<AuthoriseResult> Authorise(AuthoriseRequest request);
    }

    public enum AuthoriseResult
    {
        Approved,
        Denied
    }

    public record Metadata(DateTimeOffset Timestamp, string ExternalId);

    public record AuthoriseRequest(Amount Amount, Card Card, Merchant Merchant, Metadata Metadata);

    public record AuthoriseResponse(string Id, AuthoriseResult Result);
}
