# Authentication 

On the main page of the application ( the first page that asks for your password) we do checks.

If an account is present on the database we must do a first time user procedure:

## First time user sign in procedure

1. We ask user for a new password (And check the password if it is suitable enough).
2. We create the new account and add it to the database.
	- A random generated IV will be chosen for the whole database encryption system. This is stored unencrypted.
	- A hash will be generated from the master password via the argon2 hashing algorithm.
	- To encrypt things inside the database a key is also needed ( For the AES-256 algorithm). To do this a function salts the master password with the database IV and gets a SHA-256 hash from it and now this hash is usable as an AES-256 key for encrypting the things inside the database.
	- A new UID will be generated for the user account.
		- **Note**: The UID should be encrypted before being put inside the Database. 
		
