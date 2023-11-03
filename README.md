# POCK 8

### Setup
Configure `AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_API_KEY` environment variables first.

### Idea

The idea is that we can't give the whole CSV to the AI. So instead we only give the first 5 
lines and a list of possible column names and let the AI do the matching.

Once we have the names of the columns we can read the CSV safely.

### settings.json

I made a file (`ressources/settings.json`) in which we can put different settings for the AI. <br>
The settings are :
- `possibleColumns`: the list of possible column names, with a decription of what it is, so that we can name it anything.
- `helpInfo`: a list of info we want to give the AI to help it find the right columns. (example : "the tip is always smaller than the bonus")
- `outputFormat`: the output we are expecting from the AI, here it is :
	- `columns`: the list of columns names
	- `separator`: the separator used in the CSV
	- `hasHeader`: if the CSV has a header or not