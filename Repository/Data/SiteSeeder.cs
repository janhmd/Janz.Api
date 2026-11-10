using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Data;

// Boş bazada mətn səhifələri, əlaqə məlumatları və şəkilli kartlar başlanğıc dəyərlərlə yaranır, sonra admin paneldən dəyişir
public static class SiteSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.SitePages.AnyAsync())
        {
            db.SitePages.AddRange(
                new SitePage
                {
                    Slug = "delivery",
                    Title = "Delivery and payment",
                    InTerms = true,
                    InFooter = true,
                    Body = "## Delivery\nThe Janz courier delivers all over Baku in 2–3 days, free of charge. Other regions get their orders by Azerpost.\n\n## Payment\nPay by card on the site right after placing the order, from your Janz balance, or give cash or card to the courier.\n\n## Pickup\nNesimi, Masud Davudoglu 171, every day 10:00–20:00."
                },
                new SitePage
                {
                    Slug = "discounts",
                    Title = "Discount system",
                    InTerms = true,
                    InFooter = true,
                    Body = "## Levels\nThe more you order, the higher your discount level. The level and its discount are shown in your account.\n\n## Promo codes\nType a promo code in the cart and press Apply, the discount shows up in the total. New customers get a code for the first order in their account.\n\n## Gift certificates\nA gift certificate adds its amount to your Janz balance."
                },
                new SitePage
                {
                    Slug = "refunds",
                    Title = "Refunds",
                    InTerms = true,
                    InFooter = true,
                    Body = "## When you can return\nIf an order is broken, wrong or not as described, write to us and we will sort it out.\n\n## How\nWrite to us on WhatsApp with your order number and a photo. The money goes back to your Janz balance or to your card."
                },
                new SitePage
                {
                    Slug = "faq",
                    Title = "FAQ",
                    InTerms = true,
                    InFooter = true,
                    Body = "## How do I choose a coffee?\nOpen the catalog and filter by brewing method and taste, or ask the Janz assistant.\n\n## Where is my order?\nOpen your account and go to Orders: every order shows its status there.\n\n## Can I grind the beans?\nYes, choose the grind on the product page."
                },
                new SitePage
                {
                    Slug = "privacy",
                    Title = "Privacy policy",
                    InFooter = true,
                    Body = "## What we keep\nWe keep your name, email, phone, addresses and orders to deliver them and to show your history.\n\n## Who sees it\nOnly the Janz team and the courier who delivers your order. We do not sell your data.\n\n## Your data\nYou can change your details in your account or ask us to delete them."
                });
        }

        if (!await db.SiteSettings.AnyAsync())
        {
            db.SiteSettings.Add(new SiteSetting
            {
                Phone = "+994 70 217 55 57",
                Email = "janahmadap@code.edu.az",
                Address = "Nesimi, Masud Davudoglu 171",
                Instagram = "https://instagram.com/prglnv",
                WhatsApp = "https://wa.me/994702175557",
                Hours = "Every day 10:00–20:00"
            });
        }

        // Kartlar: indiki şəkil və videolar wwwroot-dadır, admin panel onları dəyişəndə yenisi /uploads altında saxlanır
        if (!await db.SiteTiles.AnyAsync())
        {
            db.SiteTiles.AddRange(
                    new SiteTile { Placement = "home", SortOrder = 1, Title = "Coffee", Link = "/Products", ImagePath = "/images/tiles/espresso.jpg", VideoPath = "/videos/espresso.mp4" },
                    new SiteTile { Placement = "home", SortOrder = 2, Title = "Syrups", Link = "/Products/Chocolate", ImagePath = "/images/tiles/choc-syrup.jpg", VideoPath = "/videos/choc-syrup.mp4" },
                    new SiteTile { Placement = "home", SortOrder = 3, Title = "Tea", Link = "/Products/Tea", ImagePath = "/images/tiles/tea.jpg", VideoPath = "/videos/tea.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 1, Title = "Coffee for espresso", Link = "?use=espresso", ImagePath = "/images/tiles/espresso.jpg", VideoPath = "/videos/espresso.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 2, Title = "Coffee for filter", Link = "?use=filter", ImagePath = "/images/tiles/filter.jpg", VideoPath = "/videos/filter.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 3, Title = "Capsules", Link = "?format=capsules", ImagePath = "/images/tiles/capsules.jpg", VideoPath = "/videos/capsules.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 4, Title = "Drip bags", Link = "?format=drip", ImagePath = "/images/tiles/drip.jpg", VideoPath = "/videos/drip.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 5, Title = "Drinks in cans", Link = "?format=cans", ImagePath = "/images/tiles/cans.jpg", VideoPath = "/videos/cans.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 6, Title = "Coffee concentrate", Link = "?format=concentrate", ImagePath = "/images/tiles/concentrate.jpg", VideoPath = "/videos/concentrate.mp4" },
                    new SiteTile { Placement = "coffee", SortOrder = 7, Title = "Instant specialty", Link = "?format=instant", ImagePath = "/images/tiles/instant.jpg", VideoPath = "/videos/instant.mp4" },
                    new SiteTile { Placement = "tea", SortOrder = 1, Title = "Green tea", Link = "?tag=green-tea", ImagePath = "/images/tiles/tea-green.jpg", VideoPath = "/videos/tea-green.mp4" },
                    new SiteTile { Placement = "tea", SortOrder = 2, Title = "Black tea", Link = "?tag=black-tea", ImagePath = "/images/tiles/tea-black.jpg", VideoPath = "/videos/tea-black.mp4" },
                    new SiteTile { Placement = "tea", SortOrder = 3, Title = "Oolong", Link = "?tag=oolong", ImagePath = "/images/tiles/tea-oolong.jpg", VideoPath = "/videos/tea-oolong.mp4" },
                    new SiteTile { Placement = "tea", SortOrder = 4, Title = "White tea", Link = "?tag=white-tea", ImagePath = "/images/tiles/tea-white.jpg", VideoPath = "/videos/tea-white.mp4" },
                    new SiteTile { Placement = "tea", SortOrder = 5, Title = "Pu-erh", Link = "?tag=pu-erh", ImagePath = "/images/tiles/tea-puerh.jpg", VideoPath = "/videos/tea-puerh.mp4" },
                    new SiteTile { Placement = "chocolate", SortOrder = 1, Title = "Chocolate bars", Link = "?tag=chocolate-bars", ImagePath = "/images/tiles/choc-bars.jpg", VideoPath = "/videos/choc-bars.mp4" },
                    new SiteTile { Placement = "chocolate", SortOrder = 2, Title = "Chocolate shavings", Link = "?tag=chocolate-shavings", ImagePath = "/images/tiles/choc-shavings.jpg", VideoPath = "/videos/choc-shavings.mp4" },
                    new SiteTile { Placement = "chocolate", SortOrder = 3, Title = "Syrups", Link = "?tag=syrups", ImagePath = "/images/tiles/choc-syrup.jpg", VideoPath = "/videos/choc-syrup.mp4" },
                    new SiteTile { Placement = "chocolate", SortOrder = 4, Title = "Dry syrups", Link = "?tag=dry-syrups", ImagePath = "/images/tiles/choc-dry-syrup.jpg", VideoPath = "/videos/choc-dry-syrup.mp4" },
                    new SiteTile { Placement = "chocolate", SortOrder = 5, Title = "Dry bases", Link = "?tag=dry-bases", ImagePath = "/images/tiles/choc-dry-base.jpg", VideoPath = "/videos/choc-dry-base.mp4" },
                    new SiteTile { Placement = "sets", SortOrder = 1, Title = "Sample boxes", Link = "?tag=sample-boxes", ImagePath = "/images/tiles/set-samples.jpg", VideoPath = "/videos/set-samples.mp4" },
                    new SiteTile { Placement = "sets", SortOrder = 2, Title = "Gift sets", Link = "?tag=gift-sets", ImagePath = "/images/tiles/set-gifts.jpg", VideoPath = "/videos/set-gifts.mp4" });
        }

        await db.SaveChangesAsync();
    }
}
