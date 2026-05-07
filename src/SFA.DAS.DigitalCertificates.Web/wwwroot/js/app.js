// AUTOCOMPLETE

let role = 'search-location';
let $input = $('input[data-role="' + role + '"]');

let additionalInputClasses = '';
if ($input.hasClass('govuk-input--error')) {
    additionalInputClasses = 'autocomplete__input--error';
}

let $submitOnConfirm = $input.data('submit-on-selection');
let $defaultValue = $input.data('default-value') || $input.val();
let $name = $input.attr('name');

if ($input.length > 0) {
    $input.wrap('<div id="autocomplete-container"></div>');
    let container = document.querySelector('#autocomplete-container');
    let apiUrl = '/locations';
    $(container).empty();
    function getSuggestions(query, updateResults) {
        let results = [];
        $.ajax({
            url: apiUrl,
            type: "get",
            dataType: 'json',
            data: { searchTerm: query }
        }).done(function (data) {
            results = data.locations.map(function (r) {
                return { name: r.name };
            });
            updateResults(results);
        });
    }
    function onConfirm(selectedItem) {
        let currentElement = this.element;

        // traverse up the DOM to find the nearest form
        while (currentElement && currentElement.tagName.toLocaleLowerCase() !== 'form') {
            currentElement = currentElement.parentElement;
        }

        if (currentElement && currentElement.tagName.toLocaleLowerCase() === 'form') {
            try {
                var addressInput = currentElement.querySelector('input[name="SelectedAddress"]');

                var addressValue = '';
                if (typeof selectedItem === 'string') {
                    addressValue = selectedItem;
                } else if (selectedItem && selectedItem.name) {
                    addressValue = selectedItem.name;
                }

                if (addressInput) addressInput.value = addressValue;
            } catch (e) {

            }

            if ($submitOnConfirm) {
                setTimeout(function () {
                    currentElement.submit();
                }, 200);
            }
        }
    }

    // Initialize accessibleAutocomplete with the custom input template
    accessibleAutocomplete({
        element: container,
        id: $input.attr('id'),
        name: $name,
        displayMenu: 'overlay',
        showNoOptionsFound: false,
        minLength: 3,
        source: getSuggestions,
        placeholder: "",
        onConfirm: onConfirm,
        defaultValue: $defaultValue,
        confirmOnBlur: false,
        inputClasses: additionalInputClasses,
        autoselect: true,
        templates: {
            inputValue: function (suggestion) {
                if (typeof suggestion === 'string') {
                    return suggestion;
                }
                return suggestion && suggestion.name ? suggestion.name : '';
            },
            suggestion: function (suggestion) {
                if (typeof suggestion === 'string') {
                    return '';
                }

                return suggestion.name;
            }
        }
    });
}

// copy-to-clipboard handler for sharing link page
document.addEventListener('DOMContentLoaded', function () {
    var copyBtn = document.getElementById('copy-link-btn');
    var webLinkInput = document.getElementById('web-link');
    if (copyBtn && webLinkInput) {
        copyBtn.style.display = 'inline-block';
        copyBtn.addEventListener('click', function () {
            webLinkInput.select();
            webLinkInput.setSelectionRange(0, 99999); // For mobile devices
            navigator.clipboard.writeText(webLinkInput.value)
                .then(function () {
                    copyBtn.textContent = 'Copied!';
                    setTimeout(function () {
                        copyBtn.textContent = 'Copy link';
                    }, 1500);
                });
        });
    }
});


// cookies
function saveCookieSettings() {
    let consentAnalyticsCookieRadioValue = document.querySelector(
        "input[name=ConsentAnalyticsCookie]:checked"
    ).value;    

    createCookie("AnalyticsConsent", consentAnalyticsCookieRadioValue);    

    if (consentAnalyticsCookieRadioValue === 'false') {
        deleteCookie('_ga');
        deleteCookie('_gid');
        deleteCookie('_gat');
    }

    document.getElementById("confirmation-banner").removeAttribute("hidden");
    window.scrollTo({ top: 0, behavior: "instant" });
}

function createCookie(cookiename, cookievalue) {
    let date = new Date();
    date.setFullYear(date.getFullYear() + 1);
    let expires = "expires=" + date.toGMTString();
    document.cookie =
        cookiename + "=" + cookievalue + ";" + expires + ";path=/;Secure";
}

function deleteCookie(name) {
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
}
