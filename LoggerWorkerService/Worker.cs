using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using LoggerWorkerService.Models;

namespace LoggerWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ApiContext _context;
        Ping pinger = null;

        public Worker()
        {
            _context = new ApiContext();
            pinger = new Ping();
        }

        public IList<TblIlogDbIp> Ips { get; set; }
        public IList<TblIlogDbIp> IIps { get; set; }
        public IList<TblIlogDbIp> SIps { get; set; }
        public bool IFlag { get; set; } = false;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Ips = await _context.GetIpsAsync();
            IIps = Ips.Where(a => a.FldIlogDbLogTypeId == 1).ToList();
            SIps = Ips.Where(a => a.FldIlogDbLogTypeId == 2).ToList();

            while (!stoppingToken.IsCancellationRequested)
            {

                if (IIps.Count > 0)
                {
                    foreach (var item in IIps)
                    {
                        //Ping to Internet Ips
                        try
                        {
                            var reply = pinger.Send(item.FldIlogDbIpAddress);
                            if (reply.Status == IPStatus.Success)
                            {
                                IFlag = true;
                            }
                            else
                            {
                                var log = new TblIlogDbLog
                                {
                                    FldIlogDbIpId = item.FldIlogDbIpId,
                                    FldIlogDbLogDateTime = DateTime.Now
                                };
                                await _context.PostLogAsync(log);
                            }
                        }
                        catch (Exception)
                        {
                            var log = new TblIlogDbLog
                            {
                                FldIlogDbIpId = item.FldIlogDbIpId,
                                FldIlogDbLogDateTime = DateTime.Now
                            };
                            await _context.PostLogAsync(log);
                        }
                    }
                }

                if (SIps.Count > 0 && IFlag)
                {
                    foreach (var item in SIps)
                    {
                        //Send Request to Server Ips
                        var addrs = item.FldIlogDbIpAddress.StartsWith("http") ? item.FldIlogDbIpAddress : "http://" + item.FldIlogDbIpAddress;
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(addrs);
                        request.Method = "HEAD";
                        try
                        {
                            var response = await request.GetResponseAsync();
                        }
                        catch (Exception)
                        {
                            var log = new TblIlogDbLog
                            {
                                FldIlogDbIpId = item.FldIlogDbIpId,
                                FldIlogDbLogDateTime = DateTime.Now
                            };
                            await _context.PostLogAsync(log);
                        }
                    }
                    IFlag = false;
                }
                await Task.Delay(60000, stoppingToken);

            }

            
        }
    }
}

