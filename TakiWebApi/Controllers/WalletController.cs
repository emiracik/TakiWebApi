using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TakiWebApi.Data;
using TakiWebApi.Models;
using System;
using System.Collections.Generic;

namespace TakiWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletRepository _walletRepo;
    public WalletController(IWalletRepository walletRepo)
    {
        _walletRepo = walletRepo;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<Wallet>> GetWallet(int userId)
    {
        var wallet = await _walletRepo.GetWalletByUserIdAsync(userId);
        if (wallet == null)
            return NotFound();
        return Ok(wallet);
    }

    [HttpGet("{userId}/transactions")]
    public async Task<ActionResult<List<WalletTransaction>>> GetLastTransactions(int userId, int count = 10)
    {
        var txs = await _walletRepo.GetLastTransactionsAsync(userId, count);
        return Ok(txs);
    }

    [HttpPost("{userId}/deposit")]
    public async Task<IActionResult> Deposit(int userId, [FromBody] decimal amount)
    {
        if (amount <= 0) return BadRequest("Tutar pozitif olmalı.");
        var wallet = await _walletRepo.GetWalletByUserIdAsync(userId) ?? await _walletRepo.CreateWalletAsync(userId);
        var tx = new WalletTransaction
        {
            WalletID = wallet.WalletID,
            UserID = userId,
            TransactionType = "Deposit",
            Amount = amount,
            Description = "Bakiye yükleme",
            TransactionDate = DateTime.Now
        };
        await _walletRepo.AddTransactionAsync(tx);
        wallet.Balance += amount;
        wallet.TotalIn += amount;
        await _walletRepo.UpdateWalletBalanceAsync(wallet.WalletID, wallet.Balance, wallet.TotalIn, wallet.TotalOut);
        return Ok();
    }

    [HttpPost("{userId}/withdraw")]
    public async Task<IActionResult> Withdraw(int userId, [FromBody] decimal amount)
    {
        if (amount <= 0) return BadRequest("Tutar pozitif olmalı.");
        var wallet = await _walletRepo.GetWalletByUserIdAsync(userId);
        if (wallet == null) return NotFound();
        if (wallet.Balance < amount) return BadRequest("Yetersiz bakiye.");
        var tx = new WalletTransaction
        {
            WalletID = wallet.WalletID,
            UserID = userId,
            TransactionType = "Withdraw",
            Amount = -amount,
            Description = "Bakiye çekme",
            TransactionDate = DateTime.Now
        };
        await _walletRepo.AddTransactionAsync(tx);
        wallet.Balance -= amount;
        wallet.TotalOut += amount;
        await _walletRepo.UpdateWalletBalanceAsync(wallet.WalletID, wallet.Balance, wallet.TotalIn, wallet.TotalOut);
        return Ok();
    }
}
}
