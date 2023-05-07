import re
from selenium import webdriver
from selenium.common.exceptions import NoSuchElementException
from selenium.webdriver.common.by import By
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.chrome.options import Options
from openpyxl import Workbook
from openpyxl.styles import Alignment
from openpyxl.utils import get_column_letter
from concurrent.futures import ThreadPoolExecutor

def ProcessAd(ad, actions, ws, maxRow):
    title = ad.find_element(By.CSS_SELECTOR, '.iva-item-titleStep-pdebR').text

    productLink = ad.find_element(By.CSS_SELECTOR, 'a').get_attribute('href')

    totalPromotion = ''
    try:
        tooltipElement = ad.find_element(By.CSS_SELECTOR, '.styles-arrow-jfRdd')
        actions.move_to_element(tooltipElement).perform()
        promotions = ad.find_elements(By.CSS_SELECTOR, '.styles-entry-MuP_G')
        for promotion in promotions:
            promotionElement = promotion.text
            if promotionElement == "Продвинуто":
                if re.search("https://www.avito.st/s/common/components/monetization/icons/web/x20",
                             promotion.find_element(By.CSS_SELECTOR, 'img').get_attribute('src')):
                    promotionElement = "Продвинуто x20"
                elif re.search("https://www.avito.st/s/common/components/monetization/icons/web/x5",
                               promotion.find_element(By.CSS_SELECTOR, 'img').get_attribute('src')):
                    promotionElement = "Продвинуто x5"
                elif re.search("https://www.avito.st/s/common/components/monetization/icons/web/x10",
                               promotion.find_element(By.CSS_SELECTOR, 'img').get_attribute('src')):
                    promotionElement = "Продвинуто x10"
                elif re.search("https://www.avito.st/s/common/components/monetization/icons/web/x15",
                               promotion.find_element(By.CSS_SELECTOR, 'img').get_attribute('src')):
                    promotionElement = "Продвинуто x15"
                elif re.search("https://www.avito.st/s/common/components/monetization/icons/web/x2.",
                               promotion.find_element(By.CSS_SELECTOR, 'img').get_attribute('src')):
                    promotionElement = "Продвинуто x2"
            totalPromotion += promotionElement + ' '
    except NoSuchElementException:
        totalPromotion = "Нет"

    sellerLink = ''
    try:
        sellerElement = ad.find_element(By.CSS_SELECTOR, '.style-title-_wK5H')
        sellerLink = ad.find_element(By.CSS_SELECTOR, 'a').get_attribute('href')
        seller = sellerElement.text
    except NoSuchElementException:
        seller = "Нет"

    adDriver = webdriver.Chrome(options=options)
    adDriver.get(productLink)
    views = adDriver.find_element(By.CSS_SELECTOR, '.style-item-footer-Ufxh_')
    views = re.search(r'\d+ просмот(?:р|ров|ра)? \(\+\d+ сегодня\)', views.text).group()
    ws.append([title, seller, totalPromotion, views])
    print(title, '|', seller, '|', totalPromotion, '|', views)

    ws.cell(row=maxRow, column=1).hyperlink = productLink
    ws.cell(row=maxRow, column=2).hyperlink = sellerLink
    maxRow += 1

    adDriver.quit()

with ThreadPoolExecutor(max_workers=5) as executor:
    wb = Workbook()
    ws = wb.active
    ws.append(['Название', 'Селлер', 'Продвижение', 'Просмотры'])
    alignment = Alignment(horizontal='center', vertical='center')
    maxRow = 2

    options = Options()
    options.add_argument('--start-maximized')

    driver = webdriver.Chrome(options=options)

    product = 'клининг'
    driver.get("https://www.avito.ru/moskva/predlozheniya_uslug/uborka_klining-ASgBAgICAUSYC7L3AQ?cd=1&q=" + product)

    actions = ActionChains(driver)
    nextButton = driver.find_element(By.CSS_SELECTOR, '.styles-module-listItem_arrow_next-GnEQw')

    # for page in range(1, int(driver.find_element(By.CSS_SELECTOR, '.styles-module-listItem_last-_ZfSe').text) + 1):
    ads = driver.find_elements(By.CSS_SELECTOR, '.iva-item-content-rejJg')
    adsChunks = [ads[i::5] for i in range(5)]
    for ad in ads:
        ProcessAd(ad, actions, ws, maxRow)
    nextButton.click()
for row in ws.rows:
    for cell in row:
        cell.alignment = alignment

for col in range(1, ws.max_column):
    col_letter = get_column_letter(col)
    ws.column_dimensions[col_letter].auto_size = True
wb.save(filename='parsing_data.xlsx')
driver.quit()