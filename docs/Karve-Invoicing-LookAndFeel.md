# Task Group A - Look and Feel
## A.1 - apply style guide
Apply all style elements from the Karve-Invoicing-UI-Style-Guide.md to all screens of the frontend
- detect user preference for dark mode or light mode and default to that value, but use a cookie to record the user's preferences
- allow for manual selection of dark/light mode using a small toggle located on a "user preferences" modal/dialog that can be opened from a settings icon on the header bar near the logout button
## A.2 - update invoices screen 
Update the main invoices screen as follows:
- display integer identifier for invoice instead of guid; this does not mean to change the primary key, instead add a new int property which should auto-increment for each newly created invoice, and ensure it is unique per Company; be sure the new database column is indexed
- be sure to use the actual invoice Status text value anywhere that references the Status, not the numerical value of the enum
- display the company name instead of the identifier on the invoices screen
- move the Create Invoice components of the invoices screen into a modal/dialog window that pops up when you click create invoice
## A.3 - update invoice detail screen
- show the integer invoice id instead of the guid
- show the company name instead of the guid
- show the customer name instead of the guid
- show the status text value instead of the enum index
## A.4 - update product screen
- move the create/edit components of the product screen into a modal/dialog window that pops up when you click create or edit
- update the currency to be a dropdown, with the alphabetically sorted options being:
  - USD - US Dollar
  - EUR - Euro
  - CNY - Chinese Yuan
  - INR - Indian Rupee
  - IDR - Indonesian Rupiah
  - JPY - Japanese Yen
  - MXN - Mexican Peso
  - BRL - Brazilian Real
- add a button for deleting a product, include a pop-up confirmation dialog; if this functionality is not implemented in the API, build out a stub endpoint in the appropriate controller and have it use a #warning build directive to indicate is it not implemented
## A.5 - update customers screen
- move the create/edit components of the customers screen into a modal/dialog window that pops up when you click create or edit
- add a button for deleting a customer, include a pop-up confirmation dialog; if this functionality is not implemented in the API, build out a stub endpoint in the appropriate controller and have it use a #warning build directive to indicate is it not implemented
## A.6 - fix login redirect
- after completing the oauth login, it currently returns the user to the /login path even though they are already authenticated. If they are authenticated, navigating to this path should redirect them to the root path.
## A.7 - Show user name
- in the header: show the user's given and family name, not their email, if those claims are part of their auth token
