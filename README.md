# SmartCache

Description

The system is a smart cache that can quickly provide information on breached email addresses, similar to https://haveibeenpwned.com/.
The system has two API endpoints:
- GET for retrieving information about an email address, with the following responses:
  - "NotFound" if the email address is not breached
  - "OK" if the email address is breached

- POST for storing breached emails, with the following responses:
  - "Created" if a new email address is successfully added to the breached list
  - "Conflict" if the email address is already in the breached list
Hashing of email addresses is implemented for additional data security but is currently commented out. It can be used with minor adaptations.

Intent

There are two approaches:
1. Using MS Orleans with grains for storing emails
2. Using a simple database
The intent is to compare the performance and effectiveness of both approaches.

Testing

Testing was conducted using:
- Postman collections to fill and read data
- Azurite running on localhost
- APIs and silo running on localhost
- Blobs and the database prefilled with data

Considerations:
- In tests, 404 (Not Found) and 409 (Conflict) errors are expected responses and not considered issues.
- Prefilled data: The blob storage contained 10 times more emails than the database.

Conclusion

The approach using MS Orleans is faster, even when prefilled with 10 times more data than the database approach.
However, some grains can become very large since certain domains have a high number of associated email addresses. To ensure even distribution, it may be beneficial to hash the entire email address and use part of the hash as the grain identifier.
