# ACID Tester
### Purpose
This app is a C# Command Line app that is used to test MarkLogic multi-statement ACID transactions.

This s a standalone tool used to test the RESTful Transaction APIs that are in the GLM-Search application.

The GLM-Search code is here. [https://github.com/garyrusso/GLM-Search](https://github.com/garyrusso/GLM-Search)

####Usage: requires at least 2 params

* params: action, file, docUri, txid
* action: (get | put | post | trans)

####action params:

* if **get** then params are: docUri, txid (optional)
* if **put** then params are: file, docUri, txid (optional)
* if **post** then params: file, txid (optional) - used to create new but this is not yet implemented.
* if **trans** then params: action (create | commit | rollback), txid (not needed for create)


####High Level Test Sequence:

1. `Get Document without transaction-id`
1. `Create New Transaction - Establishes new transactional context that is used for subsequent requests`
1. `Update Document using PUT Request with transaction-id`
1. `Get Document with transaction-id`
1. `Get Document without transaction-id`
1. `Commit Transaction`
1. `Get Document without transaction-id`


####Test Sequence:

1. `acid get /inventory/123987562062413876873155783207074035544.xml`
1. `acid trans create`
1. `acid put C:\projects\ACIDTester\acid\InventoryAirplanes.xml /inventory/123987562062413876873155783207074035544.xml 14654627132988965180_13856553052109669213`
1. `acid get /inventory/123987562062413876873155783207074035544.xml 14654627132988965180_13856553052109669213`
1. `acid get /inventory/123987562062413876873155783207074035544.xml`
1. `acid trans commit 14654627132988965180_13856553052109669213`
1. `acid get /inventory/123987562062413876873155783207074035544.xml`



