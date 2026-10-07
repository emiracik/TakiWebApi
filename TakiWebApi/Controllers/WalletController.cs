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

    // Bakiye yükleme/çekme uçları kaldırıldı: ödeme alınmadan bakiye
    // artırılabiliyordu. Yükleme, ödeme sağlayıcısının başarılı callback'i
    // üzerinden sunucu tarafında yapılacak.
}
}
