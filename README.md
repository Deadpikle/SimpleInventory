# SimpleInventory

SimpleInventory is a small Windows 7+ WPF piece of software to manage small inventories/stores where items are purchased on a regular basis. It runs on the .NET 4.7.1 framework and uses a SQLite database for data storage. SimpleInventory was built for a small school in Mondulkiri, Cambodia for use in the school store. As such, Riel is the default currency, and A4 is the default paper size.

Feature set:

* Manage your current inventory, including current stock/quantity. 
* Sort your inventory by different categories (drinks, school supplies, etc.) -- these different categories then show up as different subtotals on reports
  * Note: adding and editing categories doesn't have a UI yet but is easy to do from a SQLite editor
* Scan items in using a barcode scanner to quickly mark items as sold
* When purchasing items, you can set the quantity purchased and amount paid, and the software calculates the amount of change you need to give -- including into different currencies
  * Note: modifying currencies doesn't have a UI yet but is easy to do from a SQLite editor
* Generate PDFs of barcodes to print out for use with your barcode scanner
* Run daily or weekly reports to see how much income you generated, how many items were sold, and how much profit you made
* Run inventory reports to see how much was in stock on any given date

## What sorts of things would be nice to add?

* UI for adding and editing item categories
* UI for adding and editing currencies
* Ability to login/logout so the system tracks which users are doing (supported by the database but not by the UI)
* More reporting capabilities
* Capability to make one purchase with multiple items and designate who purchased it (makes it more like a point of sale system) -- would need to be able to turn this feature on/off
* Use LINQ or similar to clean up the manual SQLite database calls

## Can I help contribute?

Glad you asked! THere are always things that can be done on an open-source project: fix bugs, add new features, and more! Check out the issues tab of this repository and take a look at what bugs have been reported and which features have been requested. If you'd like to request a feature or file a bug, by all means, please do so!

## License

MIT License. Thanks for using the software!