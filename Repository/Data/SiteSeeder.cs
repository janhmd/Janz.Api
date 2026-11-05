using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Data;

// Boş bazada mətn səhifələri və əlaqə məlumatları başlanğıc dəyərlərlə yaranır, sonra admin paneldən dəyişir
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

        await db.SaveChangesAsync();
    }
}
