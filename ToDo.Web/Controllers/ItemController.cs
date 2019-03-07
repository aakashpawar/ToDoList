namespace ToDo.Web.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    public class ItemController : Controller
    {
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await CosmosService.GetOpenItemsAsync();
            return this.View(items);
        }

        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            return this.View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,Name,Description,Completed,Category")]
            TodoItem item)
        {
            if (this.ModelState.IsValid)
            {
                await CosmosService.CreateItemAsync(item);
                return this.RedirectToAction("Index");
            }

            return this.View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind( "Id,Name,Description,Completed,Category")]
            TodoItem item)
        {
            if (this.ModelState.IsValid)
            {
                await CosmosService.UpdateItemAsync(item);
                return this.RedirectToAction("Index");
            }

            return this.View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id, string category)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }

            var item = await CosmosService.GetTodoItemAsync(id, category);
            if (item == null)
            {
                return this.NotFound();
            }

            return this.View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id, string category)
        {
            if (id == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var item = await CosmosService.GetTodoItemAsync(id, category);
            if (item == null)
            {
                return this.NotFound();
            }

            return this.View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id, Category")] string id,
            string category)
        {
            await CosmosService.DeleteItemAsync(id, category);
            return this.RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id, string category)
        {
            var item = await CosmosService.GetTodoItemAsync(id, category);
            return this.View(item);
        }
    }
}