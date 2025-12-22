# AT Protocol Custom Handle Setup Guide

This guide explains how to set up and use a custom domain handle (e.g., `@theanarchox.net`) with the AT Protocol implementation in SocialSim.

## Overview

AT Protocol supports custom domain handles, allowing you to use your own domain name as your handle instead of a platform-specific one (like `@user.bsky.social`). This provides:

- **Brand identity**: Use your own domain as your handle
- **Portability**: Your handle stays with you across different PDS instances
- **Verification**: Your domain ownership proves your identity

## What You Need

### 1. Domain Name
- A domain you own and control (e.g., `theanarchox.net`)
- DNS management access for the domain

### 2. AT Protocol Components
- A DID (Decentralized Identifier)
- A PDS (Personal Data Server) to host your data
- DNS configuration to link your handle to your DID

### 3. Technical Setup
- Web server or DNS provider that supports TXT records
- HTTPS support for your domain (optional but recommended)

## How AT Protocol Handles Work

### Handle Resolution Process

1. **User claims**: `@theanarchox.net`
2. **Client queries**: `https://theanarchox.net/.well-known/atproto-did`
3. **Server responds**: `did:plc:your-unique-identifier`
4. **Client resolves DID**: Gets your DID document with PDS endpoint

### Two Methods for Handle Verification

#### Method 1: DNS TXT Record (Recommended)
Add a TXT record to your domain:

```
_atproto.theanarchox.net. IN TXT "did=did:plc:z72i7hdynmk6r22z27h6tvur"
```

**Advantages**:
- No web server required
- Fast propagation
- Simple to maintain

#### Method 2: HTTPS Well-Known Endpoint
Serve a file at `https://theanarchox.net/.well-known/atproto-did`:

```
did:plc:z72i7hdynmk6r22z27h6tvur
```

**Advantages**:
- Works if you already have a web server
- Can provide additional metadata

## Step-by-Step Setup

### Step 1: Create Your DID

Your DID is your unique identifier in the AT Protocol network.

**Option A: Using did:plc (Recommended for this simulation)**
```bash
# Generate a new DID:plc
# This will be implemented in Phase 3.1 of the roadmap
POST https://plc.directory/
```

**Option B: Using did:web**
```
did:web:theanarchox.net
```

### Step 2: Configure DNS

#### Using DNS TXT Record

1. Log into your DNS provider (Cloudflare, AWS Route53, etc.)
2. Add a TXT record:
   - **Name**: `_atproto` (or `_atproto.theanarchox.net` depending on provider)
   - **Type**: `TXT`
   - **Value**: `did=did:plc:your-did-here`
   - **TTL**: 3600 (1 hour)

3. Verify with dig:
```bash
dig _atproto.theanarchox.net TXT
```

Expected output:
```
_atproto.theanarchox.net. 3600 IN TXT "did=did:plc:z72i7hdynmk6r22z27h6tvur"
```

#### Using HTTPS Well-Known

1. Create directory: `.well-known/`
2. Create file: `atproto-did`
3. Content: `did:plc:your-did-here`
4. Serve at: `https://theanarchox.net/.well-known/atproto-did`
5. Ensure HTTPS is working

### Step 3: Set Up Personal Data Server (PDS)

Your PDS hosts your social data. Options:

#### Option A: Self-Host PDS
```bash
# Clone AT Protocol PDS
git clone https://github.com/bluesky-social/atproto
cd atproto/packages/pds

# Configure
cp .env.example .env
# Edit .env with your domain and DID

# Run
npm install
npm start
```

#### Option B: Use Hosted PDS
- Use Bluesky's hosted PDS: `https://bsky.social`
- Use another community PDS provider

#### Option C: SocialSim Integrated PDS
The simulation will include its own PDS implementation (Phase 3.2):
- Runs as part of the SocialSim infrastructure
- Configured for the custom domain
- Supports simulation-specific features

### Step 4: Link Your Handle to PDS

Update your PDS configuration to recognize your handle:

```json
{
  "handle": "theanarchox.net",
  "did": "did:plc:z72i7hdynmk6r22z27h6tvur",
  "pds": "https://pds.theanarchox.net"
}
```

### Step 5: Verify Setup

Test handle resolution:

```bash
# Using AT Protocol tools
curl https://theanarchox.net/.well-known/atproto-did

# Or via DNS
dig _atproto.theanarchox.net TXT
```

## SocialSim Integration

### Configuration for Simulation Agents

All simulated entities in SocialSim will use the custom handle domain. Configure in `appsettings.json`:

```json
{
  "ATProtocol": {
    "CustomDomain": "theanarchox.net",
    "HandleFormat": "{username}.theanarchox.net",
    "BaseDID": "did:plc:base-identifier",
    "PDSEndpoint": "https://pds.theanarchox.net"
  }
}
```

### Agent Handle Generation

Agents will automatically receive handles in the format:

- `agent001.theanarchox.net`
- `agent002.theanarchox.net`
- `influencer1.theanarchox.net`
- etc.

### Implementation Details

The simulation will:

1. **Generate DIDs** for each agent using the configured base
2. **Assign handles** following the pattern `{username}.{domain}`
3. **Configure DNS** (mock for simulation, real for production)
4. **Serve well-known endpoints** via the API service
5. **Resolve handles** during agent interactions

## DNS Configuration Examples

### Cloudflare
```
Type: TXT
Name: _atproto
Content: did=did:plc:z72i7hdynmk6r22z27h6tvur
TTL: Auto
```

### AWS Route53
```json
{
  "Name": "_atproto.theanarchox.net",
  "Type": "TXT",
  "TTL": 3600,
  "ResourceRecords": [
    {
      "Value": "\"did=did:plc:z72i7hdynmk6r22z27h6tvur\""
    }
  ]
}
```

### Google Cloud DNS
```bash
gcloud dns record-sets create _atproto.theanarchox.net \
  --type=TXT \
  --ttl=3600 \
  --zone=theanarchox-net \
  --rrdatas="did=did:plc:z72i7hdynmk6r22z27h6tvur"
```

## Wildcard Subdomains for Agents

To support many agents without individual DNS entries:

### DNS Wildcard
```
Type: TXT
Name: _atproto.*
Content: did=did:plc:{agent-specific-did}
```

### Programmatic DNS via API
Use your DNS provider's API to create records dynamically:

```csharp
// Example: Cloudflare API
public async Task CreateAgentDNSRecord(string username, string did)
{
    var record = new
    {
        type = "TXT",
        name = $"_atproto.{username}",
        content = $"did={did}",
        ttl = 3600
    };
    
    await cloudflareApi.CreateDnsRecord("theanarchox.net", record);
}
```

## Security Considerations

### Protect Your DID Private Keys
- Store keys securely (Azure Key Vault, AWS Secrets Manager)
- Never commit keys to version control
- Rotate keys periodically

### HTTPS Configuration
- Use Let's Encrypt for free SSL certificates
- Enable HSTS (HTTP Strict Transport Security)
- Keep certificates up to date

### DNS Security
- Enable DNSSEC for your domain
- Use strong DNS provider credentials
- Monitor for unauthorized changes

## Troubleshooting

### Handle Not Resolving

**Check DNS propagation**:
```bash
dig _atproto.theanarchox.net TXT +short
```

**Check HTTPS endpoint**:
```bash
curl https://theanarchox.net/.well-known/atproto-did
```

**Common issues**:
- DNS not propagated (wait 1-24 hours)
- TXT record format incorrect (must include `did=` prefix)
- HTTPS certificate issues
- File permissions on well-known directory

### Handle Claimed by Someone Else

If your handle appears claimed:
1. Verify DNS ownership
2. Check PDS configuration
3. Contact PDS administrator
4. Verify DID document

### Multiple DIDs for Same Handle

- Only one DID can be authoritative
- The one returned by DNS/well-known is canonical
- Update all references to use the correct DID

## Advanced Configuration

### Multi-Domain Support

Support multiple domains for different agent types:

```json
{
  "ATProtocol": {
    "Domains": [
      {
        "Domain": "theanarchox.net",
        "AgentTypes": ["influencer", "celebrity"],
        "DIDPrefix": "did:plc:inf"
      },
      {
        "Domain": "socialsim.dev",
        "AgentTypes": ["regular", "bot"],
        "DIDPrefix": "did:plc:sim"
      }
    ]
  }
}
```

### Custom PDS per Agent Type

Route different agents to different PDS instances:

```json
{
  "AgentPDSMapping": {
    "influencer": "https://pds-premium.theanarchox.net",
    "regular": "https://pds.theanarchox.net",
    "bot": "https://pds-simulation.theanarchox.net"
  }
}
```

## Testing

### Local Development

For local testing without DNS:

1. Edit `/etc/hosts`:
```
127.0.0.1 theanarchox.net
127.0.0.1 agent001.theanarchox.net
```

2. Run local PDS on localhost:3000
3. Configure simulation to use local endpoints

### Mock DNS Resolver

Implement a mock resolver for tests:

```csharp
public class MockHandleResolver : IHandleResolver
{
    private Dictionary<string, string> _handles = new()
    {
        ["theanarchox.net"] = "did:plc:base",
        ["agent001.theanarchox.net"] = "did:plc:agent001"
    };
    
    public Task<string?> ResolveHandle(string handle)
    {
        return Task.FromResult(_handles.GetValueOrDefault(handle));
    }
}
```

## References

- [AT Protocol Specification](https://atproto.com/specs/atp)
- [Handle Resolution Spec](https://atproto.com/specs/handle)
- [DID Methods](https://atproto.com/specs/did)
- [PDS Setup Guide](https://github.com/bluesky-social/atproto/tree/main/packages/pds)

## Next Steps

1. **Phase 1.3**: Design custom domain handle integration in data model
2. **Phase 3.1**: Implement DID generation and resolution
3. **Phase 3.2**: Build PDS with custom domain support
4. **Phase 3.3**: Implement handle resolution in AppView
5. **Phase 4**: Configure simulation to generate agents with custom handles

See ROADMAP.md for detailed implementation tasks.
