﻿namespace Commerce.ViewModels;

public partial record CartViewModel(ICartService CartService)
{
	public IFeed<Cart> Cart => Feed.Async(CartService.Get);
}