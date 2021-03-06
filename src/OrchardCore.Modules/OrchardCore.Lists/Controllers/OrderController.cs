using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Controllers
{
    [Admin]
    public class OrderController : Controller
    {
        private readonly IContainerService _containerService;

        public OrderController(IContainerService listPartQueryService)
        {
            _containerService = listPartQueryService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateContentItemOrders(string containerId, int oldIndex, int newIndex, PagerSlimParameters pagerSlimParameters, int pageSize)
        {
            var pager = new PagerSlim(pagerSlimParameters, pageSize);
            // Reverse pager as it represents the next page(s), rather than current page
            if (pager.Before != null && pager.After != null)
            {
                var beforeValue = int.Parse(pager.Before);
                beforeValue -= 1;
                var afterValue = int.Parse(pager.After);
                afterValue += 1;
                pager.Before = afterValue.ToString();
                pager.After = beforeValue.ToString();
            }
            else if (pager.Before != null)
            {
                var beforeValue = int.Parse(pager.Before);
                beforeValue -= 1;
                pager.Before = null;
                pager.After = beforeValue.ToString();
            }
            else if (pager.After != null)
            {
                var afterValue = int.Parse(pager.After);
                afterValue += 1;
                pager.After = null;
                pager.Before = afterValue.ToString();
            }

            // Include draft items.
            var pageOfContentItems = (await _containerService.QueryContainedItemsAsync(containerId, true, pager, false)).ToList();
            if (pageOfContentItems == null || !pageOfContentItems.Any())
            {
                return NotFound();
            }

            var currentOrderOfFirstItem = pageOfContentItems.FirstOrDefault().As<ContainedPart>().Order;

            var contentItem = pageOfContentItems[oldIndex];
            pageOfContentItems.Remove(contentItem);
            pageOfContentItems.Insert(newIndex, contentItem);

            await _containerService.UpdateContentItemOrdersAsync(pageOfContentItems, currentOrderOfFirstItem);

            return Ok();
        }
    }
}
