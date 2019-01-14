# SimpleInventory

SimpleInventory is a small Windows 7+ WPF software application to manage small inventories/stores where items are purchased on a regular basis. It runs on the .NET 4.7.1 framework and uses a SQLite database for data storage. SimpleInventory was built for a small school in Mondulkiri, Cambodia for use in the school store. Because of the school's location, Riel is the default currency, and A4 is the default paper size.

Feature set:

* Manage your current inventory, including current stock/quantity
* Sort your inventory into different categories (drinks, school supplies, etc.) -- these different categories then show up as different subtotals on reports
* Scan items in using a barcode scanner to quickly mark items as sold
* When purchasing items, you can set the quantity purchased and amount paid, and the software calculates the amount of change you need to give -- including into different currencies
  * Note: modifying currencies doesn't have a UI yet but is easy to do from a SQLite editor
* Generate PDFs of barcodes to print out for use with your barcode scanner
* Run daily or weekly reports to see how much income you generated, how many items were sold, and how much profit you made
* Run inventory reports to see how much was in stock on any given date
* View details on when items were sold or when the quantity of an item was adjusted down to the second

## What sorts of things would be nice to add?

* UI for adding and editing currencies
* Search field on inventory screen
* Ability to login/logout so the system tracks which users are doing what things (supported by the database but not by the UI and only somewhat by the view model code)
  * Also need to add/edit/update these users
  * Add user permission levels so that people who just run the scanning can't view all the reports/extra info?
* More reporting capabilities (details on items purchased including timestamps, etc.)
* Capability to make one purchase with multiple items and designate who purchased it (makes it more like a point of sale system) -- would need to be able to turn this feature on/off
* Use LINQ or similar to clean up the manual SQLite database calls
* Settings to change default paper size, date format, etc. throughout the app

## Can I help contribute?

Glad you asked! There are always things that can be done on an open-source project: fix bugs, add new features, and more! Check out the issues tab of this repository and take a look at what bugs have been reported and which features have been requested. If you'd like to request a feature or file a bug, by all means, please do so!

## License

MIT License. Thanks for using the software!