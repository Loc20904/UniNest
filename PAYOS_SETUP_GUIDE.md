# PayOS Premium Payment Integration Setup Guide

## Overview
This guide explains how to set up PayOS payment processing for the UniNest Premium subscription system with two plans:
- Monthly Plan: 50.000₫/month (recurring)
- Yearly Plan: 500.000₫/year (one-time)

PayOS is a popular Vietnamese payment gateway that supports multiple payment methods including cards, e-wallets, and bank transfers.

## Step 1: Create PayOS Account
1. Go to [payos.vn](https://payos.vn)
2. Click "Đăng Ký" (Sign Up)
3. Complete registration with business information
4. Verify your account via email

## Step 2: Get PayOS API Credentials
1. After login, go to **Settings** → **API**
2. You'll see three credentials:
   - **Client ID** (starts with merchant ID)
   - **API Key** (secret key)
   - **Checksum Key** (webhook verification)
3. Copy all three keys

## Step 3: Configure Backend (UniNestBE)

### Update appsettings.json
Replace the placeholder values in `appsettings.json`:

```json
"PayOS": {
    "ClientId": "your_payos_client_id",
    "ApiKey": "your_payos_api_key",
    "ChecksumKey": "your_payos_checksum_key",
    "SuccessUrl": "http://localhost:3000/premium-success",
    "CancelUrl": "http://localhost:3000/premium-info"
}
```

### Update URLs for Production
When deploying to production, update these URLs:
- `SuccessUrl`: `https://yourdomain.com/premium-success`
- `CancelUrl`: `https://yourdomain.com/premium-info`

## Step 4: Backend Implementation (Already Done)

### Files Updated:
1. **UniNestBE.csproj** - Removed Stripe.net (not needed for PayOS)
2. **Program.cs** - Added `builder.Services.AddHttpClient();`
3. **appsettings.json** - Added PayOS configuration
4. **Controllers/PaymentController.cs** - Completely rewritten for PayOS:
   - `CreatePayosPayment()` - Creates PayOS payment order
   - `ConfirmPayosPayment()` - Verifies payment status

## Step 5: Frontend Implementation (Already Done)

### Files Updated:
1. **Pages/PremiumInfo.razor**
   - Changed prices to VND (50.000₫ and 500.000₫)
   - Updated payment button text to "Thanh toán PayOS"
   - Uses new API endpoint: `/api/payment/create-payos-payment`

2. **Pages/PremiumSuccess.razor**
   - Handles PayOS return with `orderCode` parameter
   - Uses new endpoint: `/api/payment/confirm-payos-payment`

## Step 6: Environment Setup

### Required Changes in appsettings.json

```json
{
    "PayOS": {
        "ClientId": "PASTE_YOUR_CLIENT_ID_HERE",
        "ApiKey": "PASTE_YOUR_API_KEY_HERE",
        "ChecksumKey": "PASTE_YOUR_CHECKSUM_KEY_HERE",
        "SuccessUrl": "http://localhost:3000/premium-success",
        "CancelUrl": "http://localhost:3000/premium-info"
    }
}
```

### For Blazor Frontend (UniNestFE)
No additional configuration needed - it uses the backend API endpoints.

## Step 7: Test Payment Flow

### Local Testing
1. Run the backend: `dotnet run`
2. Run the frontend: `dotnet watch` in UniNestFE
3. Navigate to `/premium-info`
4. Click "Thanh toán PayOS" on either plan
5. Use PayOS test credentials (provided in PayOS Admin)

### PayOS Test Cards
Contact PayOS support for test card numbers for your specific account.

## API Endpoints

### Backend API

#### 1. Create PayOS Payment
**POST** `/api/payment/create-payos-payment`
- **Requires**: Authentication
- **Body**:
```json
{
    "planType": "monthly|yearly",
    "amount": 50000
}
```
- **Response**:
```json
{
    "checkoutUrl": "https://checkout.payos.vn/...",
    "orderCode": "UNINEST1234567890"
}
```

#### 2. Confirm PayOS Payment
**POST** `/api/payment/confirm-payos-payment`
- **Requires**: Authentication
- **Body**: `"UNINEST1234567890"`
- **Response**:
```json
{
    "success": true,
    "message": "Premium subscription activated for monthly plan",
    "planType": "monthly",
    "expiresAt": "2026-04-31T00:00:00Z"
}
```

## Database Changes

The `Users` table already has these fields:
- `IsPremium` (bit): Indicates if user is premium
- `PremiumExpiryDate` (datetime): When premium expires

These are automatically updated when payment is confirmed.

## Pricing Structure

| Plan | Amount | Duration | Price | Savings |
|------|--------|----------|-------|---------|
| Monthly | 50.000₫ | 1 month | Per month | - |
| Yearly | 500.000₫ | 1 year | Per year | 50.000₫ |

## Setup Checklist

- [ ] PayOS account created
- [ ] API credentials copied
- [ ] appsettings.json updated with PayOS credentials
- [ ] Backend code compiled successfully
- [ ] Frontend code compiled successfully
- [ ] IHttpClientFactory added to Program.cs
- [ ] Can navigate to /premium-info
- [ ] Both package cards display correct VND prices
- [ ] Can click "Thanh toán PayOS" button
- [ ] PayOS checkout page loads
- [ ] Test payment completes
- [ ] Database updated (user.IsPremium = true)
- [ ] Premium features are unlocked

## Production Deployment

### Steps:
1. Obtain production PayOS credentials from PayOS Admin
2. Update `appsettings.Production.json` with production credentials
3. Update success/cancel URLs to production domain
4. Deploy backend and frontend to production server
5. Test with small payment before going live

### Production Configuration Template
```json
{
    "PayOS": {
        "ClientId": "production_client_id",
        "ApiKey": "production_api_key",
        "ChecksumKey": "production_checksum_key",
        "SuccessUrl": "https://yourdomain.com/premium-success",
        "CancelUrl": "https://yourdomain.com/premium-info"
    }
}
```

## Webhook Setup (Optional but Recommended)

### 1. Configure Webhook in PayOS Admin
- Go to **Settings** → **Webhooks**
- Add webhook URL: `https://yourdomain.com/api/payment/webhook`
- Events to enable:
  - Order.Created
  - Order.Completed
  - Order.Cancelled

### 2. Implement Webhook Handler in Backend
Create `Controllers/PaymentWebhookController.cs` for webhook processing

## Support & Troubleshooting

### "Missing PayOS credentials"
- Verify credentials are correctly copied in appsettings.json
- Ensure no extra spaces or quotes in the configuration

### "Payment link creation failed"
- Check network tab in browser dev tools
- Verify API endpoint is accessible
- Check backend logs for detailed error
- Ensure IHttpClientFactory is registered in Program.cs

### "User not authenticated"
- Ensure user is logged in before accessing /premium-info
- Check JWT token validity
- Verify authentication middleware is properly configured

### "Payment succeeds but user not marked premium"
- Check database connection
- Verify User entity has IsPremium and PremiumExpiryDate fields
- Check PaymentController logs
- Ensure confirm-payos-payment endpoint is called

## Additional Resources

- [PayOS Documentation](https://docs.payos.vn)
- [PayOS API Reference](https://docs.payos.vn/api)
- [PayOS Admin Dashboard](https://admin.payos.vn)
- [PayOS Support](https://support.payos.vn)

## Comparison: PayOS vs Other Gateways

| Feature | PayOS | Stripe | Others |
|---------|-------|--------|--------|
| Vietnam Support | ✅ Yes | Limited | Varies |
| Local Payments | ✅ Yes | Limited | Varies |
| Setup Difficulty | Easy | Medium | Varies |
| Monthly Cost | Free | No (% per transaction) | Varies |
| Payment Methods | Multiple | Cards only | Varies |

## Next Steps

1. Create PayOS account and get credentials
2. Update `appsettings.json` with credentials
3. Test locally with test credentials
4. Deploy to staging environment
5. Test in staging
6. Deploy to production with production credentials
7. Monitor first few transactions
8. Consider implementing webhook for real-time updates

---

**Last Updated**: March 31, 2026
**Payment Gateway**: PayOS (Vietnam)
**Status**: ✅ Ready for deployment
