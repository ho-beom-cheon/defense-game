# Privacy Checklist

## Current v1.0 Candidate

- No account system
- No login
- No server API
- No Firebase
- No analytics SDK
- No ad SDK
- No billing SDK
- No cloud save
- Local JSON save only

## Data Stored Locally

The local save can include:

- Total gold
- Cleared stages
- Unlocked stages
- Upgrade levels
- Formation slots
- Last selected stage
- Tutorial completion flag
- Save version

## Required Manual Review

- Confirm Google Play Data Safety answers before store submission.
- Confirm whether a privacy policy URL is required even without data collection.
- Confirm Android permissions in the final generated manifest.

## v1.1 Notes

If rewarded ads or billing are added later, update this checklist and add the required SDK disclosures before release.
